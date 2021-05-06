using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using App.IccPortal.Tests;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Icc.WebApp.Tests
{
    public class WorkflowControllerTests
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public WorkflowControllerTests()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [Fact]
        public async Task PutPubTek_ReturnsUnauthorized_When_NotAuthorized()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJ",
                SelectedDate = DateTime.Today
            };

            var client = _factory.CreateClient();


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
                SelectedDate = DateTime.Today
            };

            var client = _factory.WithWebHostBuilder(builder =>
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
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.False(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsOkResult_When_6Digit_PubTEK_IsValid()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJ",
                SelectedDate = DateTime.Today
            };

            var client = _factory.WithWebHostBuilder(builder =>
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
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.True(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsOkResult_When_7Digit_PubTEK_IsValid()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJQ",
                SelectedDate = DateTime.Today
            };

            var client = _factory.WithWebHostBuilder(builder =>
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
                SelectedDate = DateTime.Today
            };

            var client = _factory.WithWebHostBuilder(builder =>
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

            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.False(result.Valid);
        }
    }
}