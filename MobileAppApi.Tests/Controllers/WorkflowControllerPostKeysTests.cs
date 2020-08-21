// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi;

namespace MobileAppApi.Tests.Controllers
{
    [TestClass]
    public class WorkflowControllerPostKeysTests : WebApplicationFactory<Startup>
    {
        private readonly byte[] _Key = Convert.FromBase64String(@"PwMcyc8EXF//Qkye1Vl2S6oCOo9HFS7E7vw7y9GOzJk=");
        private WebApplicationFactory<Startup> _Factory;
        private readonly byte[] _BucketId = Convert.FromBase64String(@"idlVmyDGeAXTyaNN06Uejy6tLgkgWtj32sLRJm/OuP8=");
        private DbConnection _Connection;
        private WorkflowDbContext _DbContext;

        [TestInitialize]
        public async Task InitializeAsync()
        {
            _Connection = new SqliteConnection("Data Source=:memory:");
            _DbContext = new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlite(_Connection).Options);

            _Factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(sp =>
                    {
                        var context =
                            new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlite(_Connection).Options);
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
            await _Connection.OpenAsync();
            await _DbContext.Database.EnsureCreatedAsync();
            // ReSharper disable once MethodHasAsyncOverload

            _DbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = _BucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = _Key,
            });
            await _DbContext.SaveChangesAsync();
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await _DbContext.DisposeAsync();
            await _Connection.CloseAsync();
            await _Connection.DisposeAsync();
        }

        [TestMethod]
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
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
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
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
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
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
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
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode); //All coerced by middleware to 200 now.
            Assert.AreEqual(0, items.Count);
        }
    }
}