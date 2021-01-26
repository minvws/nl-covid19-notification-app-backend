// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Collection(nameof(WorkflowControllerPostKeysTests))]
    [ExclusivelyUses(nameof(WorkflowControllerPostKeysTests))]
    public abstract class WorkflowControllerPostKeysTests : WebApplicationFactory<Startup>, IDisposable
    {
        private readonly byte[] _Key = Convert.FromBase64String(@"PwMcyc8EXF//Qkye1Vl2S6oCOo9HFS7E7vw7y9GOzJk=");
        private readonly WebApplicationFactory<Startup> _Factory;
        private readonly byte[] _BucketId = Convert.FromBase64String(@"idlVmyDGeAXTyaNN06Uejy6tLgkgWtj32sLRJm/OuP8=");
        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbProvider;

        protected WorkflowControllerPostKeysTests(IDbProvider<WorkflowDbContext> workflowDbProvider)
        {
            _WorkflowDbProvider = workflowDbProvider;
            _Factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(sp => _WorkflowDbProvider.CreateNewWithTx());
                    services.AddTransient<DecoyTimeAggregatorAttribute>();
                });

                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Validation:TemporaryExposureKey:RollingPeriod:Min"] = "1",
                        ["Validation:TemporaryExposureKey:RollingPeriod:Max"] = "256",
                        ["Workflow:ResponsePadding:ByteCount:Min"] = "8",
                        ["Workflow:ResponsePadding:ByteCount:Max"] = "64",
                    });
                });
            });
            
            var dbContext = _WorkflowDbProvider.CreateNew();
            dbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = _BucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = _Key,
            });
            dbContext.SaveChanges();
        }

        void IDisposable.Dispose()
        {
            base.Dispose();
            _WorkflowDbProvider.Dispose();
        }

        [Fact]
        [ExclusivelyUses(nameof(WorkflowControllerPostKeysTests))]
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
            var items = await _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        [ExclusivelyUses(nameof(WorkflowControllerPostKeysTests))]
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
            var items = await _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        [ExclusivelyUses(nameof(WorkflowControllerPostKeysTests))]
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
            var items = await _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        [ExclusivelyUses(nameof(WorkflowControllerPostKeysTests))]
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
            var items = await _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode); //All coerced by middleware to 200 now.
            Assert.Empty(items);
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
