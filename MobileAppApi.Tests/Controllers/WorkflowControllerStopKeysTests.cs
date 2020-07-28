// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi;

namespace MobileAppApi.Tests.Controllers
{
    [TestClass]
    public class WorkflowControllerStopKeysTests : WebApplicationFactory<Startup>
    {
        private SqliteConnection _Connection;
        private WorkflowDbContext _DbContext;
        private WebApplicationFactory<Startup> _Factory;

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
            });

            await _Connection.OpenAsync();
            await _DbContext.Database.EnsureCreatedAsync();
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await _DbContext.DisposeAsync();
            await _Connection.CloseAsync();
            await _Connection.DisposeAsync();
        }

        [TestMethod]
        public async Task PostStopKeysTest()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/stopkeys", null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Ignore("Not implemented yet")]
        [TestMethod]
        public async Task PaddingTest()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/stopkeys", null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            // Values taken from https://github.com/minvws/nl-covid19-notification-app-coordination/blob/master/architecture/Traffic%20Analysis%20Mitigation%20With%20Decoys.md
            // TODO: Should move these values to theory
            Assert.IsTrue((result.Content.Headers.ContentLength ?? 0) >= 1800);
            Assert.IsTrue((result.Content.Headers.ContentLength ?? 0) <= 17000);
        }

        /// <summary>
        /// Tests whether or not the response is delayed by at least the minimum time.
        /// </summary>
        /// <remarks>
        /// Testing the maximum delay time is difficult as it does not include the standard processing time of the
        /// HTTP request by Kestrel.
        ///
        /// TODO: Not sure what the right parameters should be here.
        /// </remarks>
        /// <param name="min">Minimum wait time</param>
        [DataTestMethod]
        [DataRow(100L)]
        [DataRow(200L)]
        [DataRow(300L)]
        [DataRow(50L)]
        [DataRow(10L)]
        public async Task DelayTest(long min)
        {
            // Arrange
            var sw = new Stopwatch();
            var client = _Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["MinimumDelayInMilliseconds"] = $"{min}",
                        ["MaximumDelayInMilliseconds"] = $"{min + 1}",
                    });
                });
            }).CreateClient();

            // Act
            sw.Start();
            var result = await client.PostAsync("v1/stopkeys", null);
            sw.Stop();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(sw.ElapsedMilliseconds > min);
        }
    }
}