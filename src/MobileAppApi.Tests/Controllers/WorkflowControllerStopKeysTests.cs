// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    public class WorkflowControllerStopKeysTests : WebApplicationFactory<Startup>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _Factory;
        private readonly Mock<IDecoyTimeCalculator> _MockTimeCalculator = new Mock<IDecoyTimeCalculator>();

        public WorkflowControllerStopKeysTests()
        {
            _Factory = WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(services => {

                        services.Replace(new ServiceDescriptor(typeof(IDecoyTimeCalculator), _MockTimeCalculator.Object));
                        services.AddTransient<DecoyTimeGeneratorAttribute>();
                        services.AddTransient<DecoyTimeAggregatorAttribute>();
                        services.AddTransient<ResponsePaddingFilterAttribute>();

                        // Remove the app's ApplicationDbContext registration.
                        var descriptor = services.SingleOrDefault(d => d.ServiceType.BaseType == typeof(DbContext));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add ApplicationDbContext using an in-memory database for testing.
                        services.AddDbContext<WorkflowDbContext>((options, context) =>
                        {
                            context.UseInMemoryDatabase("InMemoryDbForTesting");
                        });
                    });
                });
        }

        [Fact]
        public async Task PostStopKeysTest()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/stopkeys", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Theory]
        [InlineData(42)]
        [InlineData(343)]
        [InlineData(2416)]
        [InlineData(10000)]
        public async Task DelayTest(int delayMs)
        {
            // Arrange
            var sw = new Stopwatch();
            _MockTimeCalculator.Setup(x => x.GetDelay())
                .Returns(TimeSpan.FromMilliseconds(delayMs));

            var client = _Factory.CreateClient();

            // Act
            sw.Start();
            var result = await client.PostAsync("v1/stopkeys", null);
            sw.Stop();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(sw.ElapsedMilliseconds >= delayMs);
        }

        // Ticket raised to move/refactor, this tests the delay feature not if it's enabled for an endpoint
        // Ticket raised to implement this as a unit test on ResponsePaddingFilter
        [Theory]
        [InlineData(8, 16)]
        [InlineData(32, 64)]
        [InlineData(200, 300)]
        public async Task PaddingTest(long min, long max)
        {
            // Arrange
            var client = _Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Workflow:ResponsePadding:ByteCount:Min"] = $"{min}",
                        ["Workflow:ResponsePadding:ByteCount:Max"] = $"{max}",
                    });
                });
            }).CreateClient();

            // Act
            var result = await client.PostAsync("v1/stopkeys", null);
            var padding = result.Headers.GetValues("padding").SingleOrDefault();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(padding);
            Assert.True(padding.Length >= min);
            Assert.True(padding.Length <= max);
        }

        [Fact]
        public async Task Stopkeys_has_padding()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/stopkeys", null);

            // Assert
            Assert.True(result.Headers.Contains("padding"));
        }
    }
}