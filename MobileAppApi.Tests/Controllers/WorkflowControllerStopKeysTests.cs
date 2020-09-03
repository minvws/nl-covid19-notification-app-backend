// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace MobileAppApi.Tests.Controllers
{
    public class WorkflowControllerStopKeysTests : WebApplicationFactory<Startup>, IDisposable
    {
        private WebApplicationFactory<Startup> _Factory;

        public WorkflowControllerStopKeysTests()
        {
            _Factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
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

        // Ticket raised to move/refactor, this tests the delay feature not if it's enabled for an endpoint
        /// <summary>
        /// Tests whether or not the response is delayed by at least the minimum time.
        /// </summary>
        /// <param name="min">Minimum wait time</param>
        /// <param name="max">Maximum wait time</param>
        [Theory]
        [InlineData(100L, 200L)]
        [InlineData(200L, 300L)]
        [InlineData(300L, 400L)]
        [InlineData(50L, 100L)]
        [InlineData(10L, 50L)]
        public async Task DelayTest(long min, long max)
        {
            // Arrange
            var sw = new Stopwatch();
            var client = _Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Workflow:Decoys:DelayInMilliseconds:Min"] = $"{min}",
                        ["Workflow:Decoys:DelayInMilliseconds:Max"] = $"{max}"
                    });
                });
            }).CreateClient();

            // Act
            sw.Start();
            var result = await client.PostAsync("v1/stopkeys", null);
            sw.Stop();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(sw.ElapsedMilliseconds > min);
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
                        ["Workflow:Decoys:DelayInMilliseconds:Min"] = $"0",
                        ["Workflow:Decoys:DelayInMilliseconds:Max"] = $"8"
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

        [Fact]
        public async Task Register_has_padding()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/register", null);

            // Assert
            Assert.True(result.Headers.Contains("padding"));
        }

        [Fact]
        public async Task Postkeys_has_padding()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/postkeys", null);

            // Assert
            Assert.True(result.Headers.Contains("padding"));
        }
    }
}