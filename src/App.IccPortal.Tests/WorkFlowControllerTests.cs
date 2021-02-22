using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.IccPortal;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using Xunit;

namespace App.IccPortal.Tests
{
    public class WorkflowControllerTests
    {
        private readonly WebApplicationFactory<Startup> _Factory;

        public WorkflowControllerTests()
        {
            _Factory = new WebApplicationFactory<Startup>();
        }

        [Fact]
        public async Task PostLabConfirmation_ReturnsUnauthorized_When_NotAuthorized()
        {
            // Arrange
            var args = new AuthorisationArgs
            {
                LabConfirmationId = "222222",
                DateOfSymptomsOnset = DateTime.Today
            };

            var client = _Factory.CreateClient();


            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var result = await client.PostAsync($"{EndPointNames.CaregiversPortalApi.LabConfirmation}", content, token);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task PostAuthorise_ReturnsOkResult_When_ConfirmationId_IsValid()
        {
            // Arrange
            var args = new AuthorisationArgs
            {
                LabConfirmationId = "222222",
                DateOfSymptomsOnset = DateTime.Today
            };

            var client = _Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType.Name == nameof(RestApiClient));
                    services.Remove(descriptor);

                    services.AddHttpClient<IRestApiClient, FakeRestApiClient>();
                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                });
            })
                .CreateClient();


            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var result = await client.PostAsync($"{EndPointNames.CaregiversPortalApi.LabConfirmation}", content, token);
            var stringValue = await result.Content.ReadAsStringAsync();
            var authorisationResponse = JsonSerializer.Deserialize<AuthorisationResponse>(stringValue, new JsonSerializerOptions{PropertyNameCaseInsensitive = true});

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(authorisationResponse.Valid);
        }

        [Fact]
        public async Task PostAuthorise_ReturnsBadRequestResult_When_ConfirmationId_IsNotValid()
        {
            // Arrange
            var args = new AuthorisationArgs
            {
                LabConfirmationId = "111111",
                DateOfSymptomsOnset = DateTime.Today
            };

            var client = _Factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType.Name == nameof(RestApiClient));

                        services.Remove(descriptor);

                        services.AddHttpClient<IRestApiClient, FakeRestApiClient>();
                        services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                    });
                })
                .CreateClient();


            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var result = await client.PostAsync($"{EndPointNames.CaregiversPortalApi.LabConfirmation}", content, token);
            var stringValue = await result.Content.ReadAsStringAsync();
            var authorisationResponse = JsonSerializer.Deserialize<AuthorisationResponse>(stringValue, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.False(authorisationResponse.Valid);
        }
    }
}