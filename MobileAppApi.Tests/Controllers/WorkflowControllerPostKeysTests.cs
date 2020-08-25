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
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace MobileAppApi.Tests.Controllers
{
    public class WorkflowControllerPostKeysTests : WebApplicationFactory<Startup>, IDisposable
    {
        private readonly byte[] _Key = Convert.FromBase64String(@"PwMcyc8EXF//Qkye1Vl2S6oCOo9HFS7E7vw7y9GOzJk=");
        private WebApplicationFactory<Startup> _Factory;
        private readonly byte[] _BucketId = Convert.FromBase64String(@"idlVmyDGeAXTyaNN06Uejy6tLgkgWtj32sLRJm/OuP8=");
        private WorkflowDbContext _DbContext;

        public WorkflowControllerPostKeysTests()
        {
            Func<WorkflowDbContext> dbcFac = () => new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlServer("Data Source=.;Database=WorkflowControllerPostKeysTests2;Integrated Security=True").Options);
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

                });
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Validation:TemporaryExposureKey:RollingPeriod:Min"] = "1",
                        ["Validation:TemporaryExposureKey:RollingPeriod:Max"] = "256"
                    });
                });
            });
            _DbContext.Database.EnsureDeleted();
            _DbContext.Database.EnsureCreated();

            _DbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = _BucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = _Key,
            });
            _DbContext.SaveChanges();
        }

        void IDisposable.Dispose()
        {
            base.Dispose();

             _DbContext.Dispose();
        }

        [Fact]
        public async Task PostWorkflowTest_InvalidSignature()
        {
            // Arrange
            var client = _Factory.CreateClient();
            await using var inputStream =
                Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Resources.payload.json");
            var data = inputStream.ToArray();
            var signature = HttpUtility.UrlEncode(HmacSigner.Sign(new byte[] { 0 }, data));
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Act
            var result = await client.PostAsync($"v1/postkeys?sig={signature}", content);

            // Assert
            var items = await _DbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        public async Task PostWorkflowTest_ScriptInjectionInSignature()
        {
            // Arrange
            var client = _Factory.CreateClient();
            await using var inputStream =
                Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Resources.payload.json");
            var data = inputStream.ToArray();
            var signature = "<script>alert()</script>";
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Act
            var result = await client.PostAsync($"v1/postkeys?sig={signature}", content);

            // Assert
            var items = await _DbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        public async Task PostWorkflowTest_NullSignature()
        {
            // Arrange
            var client = _Factory.CreateClient();
            await using var inputStream =
                Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Resources.payload.json");
            var data = inputStream.ToArray();
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Act
            var result = await client.PostAsync("v1/postkeys", content);

            // Assert
            var items = await _DbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        public async Task PostWorkflowTest_EmptySignature()
        {
            // Arrange
            var client = _Factory.CreateClient();
            await using var inputStream =
                Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Resources.payload.json");
            var data = inputStream.ToArray();
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Act
            var result = await client.PostAsync($"v1/postkeys?sig={string.Empty}", content);

            // Assert
            var items = await _DbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode); //All coerced by middleware to 200 now.
            Assert.Empty(items);
        }
    }
}