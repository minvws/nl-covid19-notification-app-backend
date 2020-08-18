// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
using TheIdentityHub.AspNetCore.Authentication;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Icc
{
    [TestClass]
    public class TheIdentityHubServiceTests
    {
        private TheIdentityHubService _TheIdentityHubService;
        private IOptionsMonitor<TheIdentityHubOptions> _Options;
        private WireMockServer _Server;
        private int _TestPort = 8081;

        [TestInitialize]
        public void Initialize()
        {
            var logger = new TestLogger<TheIdentityHubService>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCustomOptions(TheIdentityHubDefaults.AuthenticationScheme, options =>
            {
                options.TheIdentityHubUrl = new Uri("http://localhost:" + _TestPort);
                options.Tenant = "ggdghornl_test";
                options.ClientId = "0";
                options.ClientSecret = "supersecret";
                options.CallbackPath = "/signin-identityhub";
                options.Backchannel = new HttpClient();
            });
            var builder = serviceCollection.BuildServiceProvider();
            _Options = builder.GetService<IOptionsMonitor<TheIdentityHubOptions>>();

            _TheIdentityHubService = new TheIdentityHubService(_Options, logger);
        }

        [TestMethod]
        public void Validate_If_Service_Is_Initialized()
        {
            Assert.IsNotNull(_TheIdentityHubService);
        }


        [TestMethod]
        public void VerifyTokenShouldReturnTrueOnValidToken()
        {
            var validToken = "valid_access_token";
            _Server = WireMockServer.Start(_TestPort);

            _Server.Given(
                    Request.Create()
                        .WithHeader("Authorization", "Bearer " + validToken)
                        .WithPath("/ggdghornl_test/oauth2/v1/verify").UsingGet()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody("{\"audience\":1234}")
                );

            Assert.IsTrue(_TheIdentityHubService.VerifyToken(validToken).Result);
            _Server.Stop();
        }

        [TestMethod]
        public void VerifyTokenShouldReturnFalseOnInValidToken()
        {
            var validToken = "invalid_access_token";
            _Server = WireMockServer.Start(_TestPort);

            _Server.Given(
                    Request.Create()
                        .WithHeader("Authorization", "Bearer " + validToken)
                        .WithPath("/ggdghornl_test/oauth2/v1/verify").UsingGet()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(401)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody("{\"error\":\"Invalid Access Token\"}")
                );

            Assert.IsFalse(_TheIdentityHubService.VerifyToken(validToken).Result);
            _Server.Stop();
        }


        [TestMethod]
        public void VerifyAccessTokenRevocation()
        {
            var validToken = "valid_access_token";
            _Server = WireMockServer.Start(_TestPort);

            _Server.Given(
                    Request.Create().UsingPost()
                        .WithHeader("Authorization", "Bearer " + validToken)
                        .WithPath("/ggdghornl_test/oauth2/v1/revoke")
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody("{\"msg\":\"Access Token revoked\"}")
                );
            Assert.IsTrue(_TheIdentityHubService.RevokeAccessToken(validToken).Result);
            Console.WriteLine(_Server.LogEntries);
            _Server.Stop();
        }

        [TestMethod]
        public void VerifyInvalidAccessTokenRevocation()
        {
            var invalidAccessToken = "invalid_access_token";
            _Server = WireMockServer.Start(_TestPort);

            _Server.Given(
                    Request.Create().UsingPost()
                        .WithHeader("Authorization", "Bearer " + invalidAccessToken)
                        .WithPath("/ggdghornl_test/oauth2/v1/revoke")
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(401)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody("{\"error\":\"Invalid AccessToken\"}")
                );
            Assert.IsFalse(_TheIdentityHubService.RevokeAccessToken(invalidAccessToken).Result);

            _Server.Stop();
        }
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomOptions(this IServiceCollection serviceCollection,
        string name, Action<TheIdentityHubOptions> options)
    {
        serviceCollection.Configure(name, options);
        return serviceCollection;
    }
}