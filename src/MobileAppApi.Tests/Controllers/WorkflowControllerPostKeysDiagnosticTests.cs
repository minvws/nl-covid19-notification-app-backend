// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

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
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Collection(nameof(WorkflowControllerPostKeysDiagnosticTests))]
    [ExclusivelyUses(nameof(WorkflowControllerPostKeysDiagnosticTests))]
    public abstract class WorkflowControllerPostKeysDiagnosticTests : WebApplicationFactory<Startup>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _Factory;
        private readonly FakeTimeProvider _FakeTimeProvider;
        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbProvider;

        private class FakeTimeProvider : IUtcDateTimeProvider
        {
            public DateTime Value { get; set; }
            public DateTime Now() => Value; //ncrunch: no coverage
            public DateTime Snapshot => Value;
        }

        protected WorkflowControllerPostKeysDiagnosticTests(IDbProvider<WorkflowDbContext> workflowDbProvider)
        {
            _WorkflowDbProvider = workflowDbProvider ?? throw new ArgumentNullException(nameof(workflowDbProvider));
            _FakeTimeProvider = new FakeTimeProvider();

            _Factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(_ => _WorkflowDbProvider.CreateNewWithTx());
                    services.AddTransient<IUtcDateTimeProvider>(x => _FakeTimeProvider);
                    services.AddTransient<DecoyTimeAggregatorAttribute>();
                });
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Workflow:PostKeys:TemporaryExposureKeys:RollingStartNumber:Min"] = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc).ToRollingStartNumber().ToString(),
                        ["Workflow:PostKeys:TemporaryExposureKeys:Count:Min"] = "0",
                    });
                });
            });
        }

        void IDisposable.Dispose()
        {
            base.Dispose();
            _WorkflowDbProvider.Dispose();
        }

        private async Task WriteBucket(byte[] bucketId)
        {
            var workflowDbContext = _WorkflowDbProvider.CreateNew();
            workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = bucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = new byte[32],
            });
            await workflowDbContext.SaveChangesAsync();
        }


        [Theory]
        [InlineData("Resources.payload-good00.json", 0, 7, 2)]
        [InlineData("Resources.payload-good01.json", 1, 7, 2)]
        [InlineData("Resources.payload-good14.json", 14, 7, 11)]
        [InlineData("Resources.payload-duplicate-TEKs-RSN-and-RP.json", 0, 7, 11)]
        [InlineData("Resources.payload-duplicate-TEKs-KeyData.json", 0, 7, 11)]
        [InlineData("Resources.payload-duplicate-TEKs-RSN.json", 13, 8, 13)]
        [InlineData("Resources.payload-ancient-TEKs.json", 1, 7, 1)]
        [ExclusivelyUses(nameof(WorkflowControllerPostKeysTests))]
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
            var items = await _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(keyCount, items.Count);
        }
    }
}