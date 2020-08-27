// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NCrunch.Framework;
using Xunit;

namespace MobileAppApi.Tests.Controllers
{
    public class WorkflowControllerPostKeysDiagnosticTests : WebApplicationFactory<Startup>, IDisposable
    {
        private WebApplicationFactory<Startup> _Factory;
        private WorkflowDbContext _DbContext;
        private FakeTimeProvider _FakeTimeProvider;

        private class FakeTimeProvider : IUtcDateTimeProvider
        {
            public DateTime Value { get; set; }
            public DateTime Now() => Value;
            public DateTime TakeSnapshot() => throw new NotImplementedException();
            public DateTime Snapshot => Value;
        }

        public WorkflowControllerPostKeysDiagnosticTests()
        {
            _FakeTimeProvider = new FakeTimeProvider();

            Func<WorkflowDbContext> dbcFac = () => new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlServer("Data Source=.;Database=WorkflowControllerPostKeysTests;Integrated Security=True").Options);
            _DbContext = dbcFac();

            _Factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(sp =>
                    {
                        var context = dbcFac();
                        context.BeginTransaction();
                        return context;
                    });

                    services.AddTransient<IUtcDateTimeProvider>(x => _FakeTimeProvider);
                });
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Workflow:PostKeys:TemporaryExposureKeys:RollingStartNumber:Min"] = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc).ToRollingStartNumber().ToString(),
                        ["Validation:TemporaryExposureKey:RollingPeriod:Min"] = "1",
                        ["Validation:TemporaryExposureKey:RollingPeriod:Max"] = "144"
                    });
                });
            });
            _DbContext.Database.EnsureDeleted();
            _DbContext.Database.EnsureCreated();
        }

        void IDisposable.Dispose()
        {
            base.Dispose();

            _DbContext.Database.EnsureDeleted();
            _DbContext.Dispose();
        }

        private async Task WriteBucket(byte[] bucketId)
        {
            _DbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = bucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = new byte[32],
            });
            await _DbContext.SaveChangesAsync();
        }


        [Theory]
        [InlineData("Resources.payload-good01.json", 1, 7, 2)]
        [InlineData("Resources.payload-good14.json", 14, 7, 11)]
        [InlineData("Resources.payload-duplicate-TEKs-RSN-and-RP.json", 0, 7, 11)]
        [InlineData("Resources.payload-duplicate-TEKs-KeyData.json", 0, 7, 11)]
        [InlineData("Resources.payload-duplicate-TEKs-RSN.json", 13, 8, 13)]
        [InlineData("Resources.payload-ancient-TEKs.json", 1, 7, 1)]
        [ExclusivelyUses("WorkflowControllerPostKeysTests")]
        public async Task PostWorkflowTest(string file, int keyCount, int mm, int dd)
        {

            _FakeTimeProvider.Value = new DateTime(2020, mm, dd, 0, 0, 0, DateTimeKind.Utc);

            // Arrange
            var client = _Factory.CreateClient();
            await using var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(file);
            var data = inputStream.ToArray();

            var args = new StandardJsonSerializer().Deserialize<PostTeksArgs>(Encoding.UTF8.GetString(data));
            await WriteBucket(Convert.FromBase64String(args.BucketId));

            var tekDates = args.Keys
                .OrderBy(x => x.RollingStartNumber)
                .Select(x => new {x, Date = x.RollingStartNumber.FromRollingStartNumber()});

            foreach (var i in tekDates)
                Trace.WriteLine($"RSN:{i.x.RollingStartNumber} Date:{i.Date:yyyy-MM-dd}.");

            var signature = HttpUtility.UrlEncode(HmacSigner.Sign(new byte[32], data));
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Act
            var result = await client.PostAsync($"v1/postkeys?sig={signature}", content);

            // Assert
            var items = await _DbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(keyCount, items.Count);
        }
    }
}