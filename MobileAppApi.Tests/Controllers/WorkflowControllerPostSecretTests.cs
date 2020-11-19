// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi;
using Xunit;

namespace MobileAppApi.Tests.Controllers
{
    //TODO Sqlite version
    [Collection(nameof(WorkflowControllerPostSecretTests))]
    [ExclusivelyUses(nameof(WorkflowControllerPostSecretTests))]
    public class WorkflowControllerPostSecretTests : WebApplicationFactory<Startup>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _Factory;
        private readonly WorkflowDbContext _DbContext;
        private readonly FakeNumberGen _FakeNumbers = new FakeNumberGen();

        public WorkflowControllerPostSecretTests()
        {
            WorkflowDbContext DbcFac() => new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlServer($"Data Source=.;Database={nameof(WorkflowControllerPostSecretTests)};Integrated Security=True").Options);
            _DbContext = DbcFac();

            _Factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(sp =>
                    {
                        var context = DbcFac();
                        context.BeginTransaction();
                        return context;
                    });


                    services.Replace(new ServiceDescriptor(typeof(IRandomNumberGenerator), _FakeNumbers));
                });
            });

            _DbContext.Database.EnsureDeleted();
            _DbContext.Database.EnsureCreated();
        }

        private class FakeNumberGen: IRandomNumberGenerator
        {
            public int Value { get; set; }

            public int Next(int min, int max) => Value;

            public byte[] NextByteArray(int length)
            {
                var buffer = new byte[length];
                buffer[0] = (byte)Value;
                return buffer;
            }
        }

        void IDisposable.Dispose()
        {
            base.Dispose();

            _DbContext.Dispose();
        }

        [Fact]
        [ExclusivelyUses("WorkflowControllerPostSecretTests")]
        public async Task PostSecretTest_EmptyDb()
        {
            // Arrange
            var client = _Factory.CreateClient();

            _FakeNumbers.Value = 1;

            // Act
            var result = await client.PostAsync("v1/register", null);

            // Assert
            var items = await _DbContext.KeyReleaseWorkflowStates.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(1, items.Count);
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

        [Fact]
        [ExclusivelyUses("WorkflowControllerPostSecretTests")]
        public async Task PostSecretTest_5RetriesAndBang()
        {

            _DbContext.KeyReleaseWorkflowStates.AddRange(Enumerable.Range(1, 5).Select(Create));
            _DbContext.SaveChanges();
            // Arrange
            var client = _Factory.CreateClient();
            _FakeNumbers.Value = 1;

            // Act
            var result = await client.PostAsync("v1/register", null);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("{\"labConfirmationId\":null,\"bucketId\":null,\"confirmationKey\":null,\"validity\":-1}", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        [ExclusivelyUses("WorkflowControllerPostSecretTests")]
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
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(6, items.Count);
        }

    }
}