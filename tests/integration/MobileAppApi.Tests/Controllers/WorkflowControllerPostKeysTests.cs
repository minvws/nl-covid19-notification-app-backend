// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Collection(nameof(WorkflowControllerPostKeysTests))]
    public abstract class WorkflowControllerPostKeysTests : WebApplicationFactory<Startup>, IDisposable
    {
        private readonly byte[] _key = Convert.FromBase64String(@"PwMcyc8EXF//Qkye1Vl2S6oCOo9HFS7E7vw7y9GOzJk=");
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly byte[] _bucketId = Convert.FromBase64String(@"idlVmyDGeAXTyaNN06Uejy6tLgkgWtj32sLRJm/OuP8=");
        private readonly WorkflowDbContext _workflowDbContext;

        protected WorkflowControllerPostKeysTests(DbContextOptions<WorkflowDbContext> workflowDbContextOptions)
        {
            _workflowDbContext = new WorkflowDbContext(workflowDbContextOptions ?? throw new ArgumentNullException(nameof(workflowDbContextOptions)));
            _workflowDbContext.Database.EnsureCreated();

            _factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<DecoyTimeAggregatorAttribute>();
                });
            });
        }

        [Fact]
        public async Task PostWorkflowTest_InvalidSignature()
        {
            // Arrange
            var client = _factory.CreateClient();
            await using var inputStream =
                Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Resources.payload.json");
            var data = inputStream.ToArray();
            var signature = HttpUtility.UrlEncode(HmacSigner.Sign(new byte[] { 0 }, data));
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = _bucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = _key,
            });
            await _workflowDbContext.SaveChangesAsync();

            // Act
            var result = await client.PostAsync($"v1/postkeys?sig={signature}", content);

            // Assert
            var items = await _workflowDbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        public async Task PostWorkflowTest_ScriptInjectionInSignature()
        {
            // Arrange
            var client = _factory.CreateClient();
            await using var inputStream =
                Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Resources.payload.json");
            var data = inputStream.ToArray();
            var signature = "<script>alert()</script>";
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");


            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = _bucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = _key,
            });
            await _workflowDbContext.SaveChangesAsync();

            // Act
            var result = await client.PostAsync($"v1/postkeys?sig={signature}", content);

            // Assert
            var items = await _workflowDbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        public async Task PostWorkflowTest_NullSignature()
        {
            // Arrange
            var client = _factory.CreateClient();
            await using var inputStream =
                Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Resources.payload.json");
            var data = inputStream.ToArray();
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = _bucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = _key,
            });
            await _workflowDbContext.SaveChangesAsync();

            // Act
            var result = await client.PostAsync("v1/postkeys", content);

            // Assert
            var items = await _workflowDbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(items);
        }

        [Fact]
        public async Task PostWorkflowTest_EmptySignature()
        {
            // Arrange
            var client = _factory.CreateClient();
            await using var inputStream =
                Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Resources.payload.json");
            var data = inputStream.ToArray();
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = _bucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = _key,
            });
            await _workflowDbContext.SaveChangesAsync();

            // Act
            var result = await client.PostAsync($"v1/postkeys?sig={string.Empty}", content);

            // Assert
            var items = await _workflowDbContext.TemporaryExposureKeys.ToListAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode); //All coerced by middleware to 200 now.
            Assert.Empty(items);
        }

        [Fact]
        public async Task Postkeys_has_padding()
        {
            // Arrange
            var client = _factory.CreateClient();

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                BucketId = _bucketId,
                ValidUntil = DateTime.UtcNow.AddHours(1),
                Created = DateTime.UtcNow,
                ConfirmationKey = _key,
            });
            await _workflowDbContext.SaveChangesAsync();

            // Act
            var result = await client.PostAsync("v1/postkeys", null);

            // Assert
            Assert.True(result.Headers.Contains("padding"));
        }
    }
}
