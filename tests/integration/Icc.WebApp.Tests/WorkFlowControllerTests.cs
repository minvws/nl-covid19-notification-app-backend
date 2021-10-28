// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using App.IccPortal.Tests;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Icc.WebApp.Tests
{
    public abstract class WorkflowControllerTests : WebApplicationFactory<Startup>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        protected WorkflowControllerTests(DbContextOptions<WorkflowDbContext> workflowDbContextOptions)
        {
            _factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(p => new WorkflowDbContext(workflowDbContextOptions));
                    services.AddHttpClient<IRestApiClient, FakeRestApiClient>();
                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                });
            });
        }

        [Fact]
        public async Task PutPubTek_ReturnsUnauthorized_When_NotAuthorized()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJ",
                DateOfSymptomsOnset = DateTime.Today,
                SubjectHasSymptoms = true
            };

            // Create factory without PolicyEvaluator
            var factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                });
            });
            var client = factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var result = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task PutPubTek_ReturnsFalseResult_When_5Digit_PubTEK_IsSend()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6L",
                DateOfSymptomsOnset = DateTime.Today,
                SubjectHasSymptoms = true
            };

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.False(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsFalseResult_When_6Digit_PubTEK_IsSend()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJ",
                DateOfSymptomsOnset = DateTime.Today,
                SubjectHasSymptoms = true
            };

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.False(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsOkResult_When_7Digit_PubTEK_IsValid()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJQ",
                DateOfSymptomsOnset = DateTime.Today,
                SubjectHasSymptoms = true
            };

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.True(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsOkResult_When_PubTEK_IsNotValid()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJR",
                DateOfSymptomsOnset = DateTime.Today,
                SubjectHasSymptoms = true
            };

            var client = _factory.CreateClient();


            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.False(result.Valid);
        }
    }
}
