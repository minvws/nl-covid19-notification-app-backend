// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Collection(nameof(WorkflowControllerPostSecretTests))]
    public abstract class WorkflowControllerPostSecretTests : WebApplicationFactory<Startup>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly FakeNumberGen _fakeNumbers = new FakeNumberGen();
        private readonly WorkflowDbContext _workflowDbContext;

        protected WorkflowControllerPostSecretTests(DbContextOptions<WorkflowDbContext> workflowDbContextOptions)
        {
            _workflowDbContext = new WorkflowDbContext(workflowDbContextOptions ?? throw new ArgumentNullException(nameof(workflowDbContextOptions)));
            _workflowDbContext.Database.EnsureCreated();

            _factory = WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddScoped(sp => new WorkflowDbContext(workflowDbContextOptions));
                        services.Replace(new ServiceDescriptor(typeof(IRandomNumberGenerator), _fakeNumbers));
                        services.AddTransient<DecoyTimeAggregatorAttribute>();
                    });

                    builder.ConfigureAppConfiguration((ctx, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            ["Workflow:ResponsePadding:ByteCount:Min"] = "8",
                            ["Workflow:ResponsePadding:ByteCount:Max"] = "64"
                        });
                    });
                });
        }

        private class FakeNumberGen : IRandomNumberGenerator
        {
            public int Value { get; set; } = 10;

            public int Next(int min, int max) => Value;

            public byte[] NextByteArray(int length)
            {
                var buffer = new byte[length];
                buffer[0] = (byte)Value;
                return buffer;
            }
        }

        [Theory]
        [InlineData("v1/register")]
        [InlineData("v2/register")]
        public async Task PostSecretTest_EmptyDb(string endpoint)
        {
            // Arrange
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());

            var client = _factory.CreateClient();

            _fakeNumbers.Value = 1;

            // Act
            var result = await client.PostAsync(endpoint, null);

            // Assert
            var items = await _workflowDbContext.KeyReleaseWorkflowStates.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Single(items);
        }

        private TekReleaseWorkflowStateEntity Create(int value)
        {
            var e1 = new TekReleaseWorkflowStateEntity
            {
                BucketId = new byte[32],
                ConfirmationKey = new byte[32],
                LabConfirmationId = "1"
            };

            e1.ConfirmationKey[0] = (byte)value;
            e1.BucketId[0] = (byte)value;
            e1.LabConfirmationId = $"{value}{value}{value}{value}{value}{value}";

            return e1;
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        public async Task PostSecretTest_5RetriesAndBang(string endpoint)
        {
            // Arrange
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());

            var endpointToResultMap = new Dictionary<string, string>()
            {
                { "v1", "{\"labConfirmationId\":null,\"bucketId\":null,\"confirmationKey\":null,\"validity\":-1}" },
                { "v2", "{\"ggdKey\":null,\"bucketId\":null,\"confirmationKey\":null,\"validity\":-1}"}
            };

            _workflowDbContext.KeyReleaseWorkflowStates.AddRange(Enumerable.Range(1, 5).Select(Create));
            _workflowDbContext.SaveChanges();

            var client = _factory.CreateClient();
            _fakeNumbers.Value = 1;

            // Act
            var result = await client.PostAsync($"{endpoint}/register", null);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(endpointToResultMap[endpoint], await result.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("v1/register")]
        [InlineData("v2/register")]
        public async Task PostSecret_MissThe5Existing(string endpoint)
        {
            // Arrange
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());

            _workflowDbContext.KeyReleaseWorkflowStates.AddRange(Enumerable.Range(1, 5).Select(Create));
            _workflowDbContext.SaveChanges();

            _fakeNumbers.Value = 6;

            var client = _factory.CreateClient();

            // Act
            var result = await client.PostAsync(endpoint, null);

            // Assert
            var items = await _workflowDbContext.KeyReleaseWorkflowStates.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(6, items.Count);
        }

        [Theory]
        [InlineData("v1/register")]
        [InlineData("v2/register")]
        public async Task Register_has_padding(string endpoint)
        {
            // Arrange
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());

            var client = _factory.CreateClient();

            // Act
            var result = await client.PostAsync(endpoint, null);

            // Assert
            Assert.True(result.Headers.Contains("padding"));
        }
    }
}
