// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi;

namespace MobileAppApi.Tests.Controllers
{
    [TestClass]
    public class WorkflowControllerPostSecretTests : WebApplicationFactory<Startup>
    {
        private WebApplicationFactory<Startup> _Factory;
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
        public async Task PostSecretTest()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/register", null);

            // Assert
            var items = await _DbContext.KeyReleaseWorkflowStates.ToListAsync();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, items.Count);
        }
    }
}