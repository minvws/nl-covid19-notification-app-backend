// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
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
            //_Connection = new SqliteConnection("Data Source=:memory:");
            //_DbContext = new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlite(_Connection).Options);

            _Connection = new SqlConnection("Data Source=.;Database=WorkflowControllerPostSecretTests;Integrated Security=True");
            _DbContext = new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlServer(_Connection).Options);

            _FakeNumbers = new FakeNumberGen();

            _Factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(sp =>
                    {
                        var context =
                            //new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlite(_Connection).Options);
                            new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlServer(_Connection).Options);
                        context.BeginTransaction();
                        return context;
                    });


                    services.Replace(new ServiceDescriptor(typeof(IRandomNumberGenerator), _FakeNumbers));
                });
            });

            await _DbContext.Database.EnsureDeletedAsync();
            await _DbContext.Database.EnsureCreatedAsync();
            //await _Connection.OpenAsync();
        }

        private FakeNumberGen _FakeNumbers;

        private class FakeNumberGen: IRandomNumberGenerator
        {
            public int Value { get; set; }

            public int Next(int min, int max) => Value;

            public byte[] NextByteArray(int length)
            {
                if (length <= 0)
                    throw new ArgumentOutOfRangeException(nameof(length));

                //if (length == 0)
                //    return new byte[0];

                var buffer = new byte[length];
                buffer[0] = (byte)Value;
                return buffer;
            }
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await _DbContext.DisposeAsync();
            await _Connection.CloseAsync();
            await _Connection.DisposeAsync();
        }

        [TestMethod]
        [ExclusivelyUses("DB")]
        public async Task PostSecretTest_EmptyDb()
        {
            // Arrange
            var client = _Factory.CreateClient();

            _FakeNumbers.Value = 1;

            // Act
            var result = await client.PostAsync("v1/register", null);

            // Assert
            var items = await _DbContext.KeyReleaseWorkflowStates.ToListAsync();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, items.Count);
        }


        private TekReleaseWorkflowStateEntity Create(int value)
        {
            var e1 = new TekReleaseWorkflowStateEntity
            {
                BucketId = new byte[32],
                ConfirmationKey = new byte[32],
                LabConfirmationId = "1"
            };

            e1.ConfirmationKey[0] = (byte) value;
            e1.BucketId[0] = (byte)value;
            e1.LabConfirmationId = $"{value}{value}{value}{value}{value}{value}";

            return e1;
        }

        [TestMethod]
        [ExclusivelyUses("DB")]
        public async Task PostSecretTest_5RetriesAndBang()
        {

            _DbContext.KeyReleaseWorkflowStates.AddRange(Enumerable.Range(1, 5).Select(Create));
            _DbContext.SaveChanges();
            // Arrange
            var client = _Factory.CreateClient();
            _FakeNumbers.Value = 1;

            // Act
            var result = await client.PostAsync("v1/register", null);

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual("{\"labConfirmationId\":null,\"bucketId\":null,\"confirmationKey\":null,\"validity\":-1}", await result.Content.ReadAsStringAsync());
        }

        [TestMethod]
        [ExclusivelyUses("DB")]
        public async Task PostSecret_MissThe5Existing()
        {

            _DbContext.KeyReleaseWorkflowStates.AddRange(Enumerable.Range(1, 5).Select(Create));
            _DbContext.SaveChanges();
            
            _FakeNumbers.Value = 6;
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/register", null);

            // Assert
            var items = await _DbContext.KeyReleaseWorkflowStates.ToListAsync();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(6, items.Count);
        }

    }
}