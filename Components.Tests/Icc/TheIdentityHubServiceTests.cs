// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
using System;
using System.Net.Http;
using TheIdentityHub.AspNetCore.Authentication;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Icc
{
    public class TheIdentityHubServiceTests
    {
        private ITheIdentityHubService _TheIdentityHubService;
        
        private WireMockServer _Server;

        public TheIdentityHubServiceTests()
        {
            _Server = WireMockServer.Start();
            if (_Server.Urls.Length < 1)
            {
                throw new Exception("WireMockServer not started correctly");
            }
            
            _TheIdentityHubService =  TestTheIdentityHubServiceCreator.CreateInstance(_Server);
        }

        [Fact]
        public void Validate_If_Service_Is_Initialized()
        {
            Assert.NotNull(_TheIdentityHubService);
        }


        [Fact]
        public void VerifyTokenShouldReturnTrueOnValidToken()
        {
            var validToken = "valid_access_token";

            _Server.Reset();
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

            Assert.True(_TheIdentityHubService.VerifyToken(validToken).Result);
        }

        [Fact]
        public void VerifyTokenShouldReturnFalseOnInValidToken()
        {
            var validToken = "invalid_access_token";

            _Server.Reset();
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

            Assert.False(_TheIdentityHubService.VerifyToken(validToken).Result);
        }


        [Fact]
        public void VerifyAccessTokenRevocation()
        {
            var validToken = "valid_access_token";

            _Server.Reset();
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
            Assert.True(_TheIdentityHubService.RevokeAccessToken(validToken).Result);
        }

        [Fact]
        public void VerifyInvalidAccessTokenRevocation()
        {
            var invalidAccessToken = "invalid_access_token";

            _Server.Reset();
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
            Assert.False(_TheIdentityHubService.RevokeAccessToken(invalidAccessToken).Result);
        }
    }
}