// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Icc.AuthHandlers
{
    public class JwtClaimValidatorTests
    {
        private readonly JwtClaimValidator _JwtClaimValidator;
        private readonly WireMockServer _Server;
        private readonly double _ClaimLifetimeHours = 1.0;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public JwtClaimValidatorTests()
        {
            var logger = new TestLogger<JwtClaimValidator>();
            _DateTimeProvider = new StandardUtcDateTimeProvider();
            _Server = WireMockServer.Start();
            _JwtClaimValidator =
                new JwtClaimValidator(TestTheIdentityHubServiceCreator.CreateInstance(_Server), logger,
                    _DateTimeProvider,
                    new HardCodedIccPortalConfig(null, "http://test.test", "test_secret123", _ClaimLifetimeHours, true));
        }

        [Fact]
        public async Task ValidateShouldReturnTrueOnValidTIHJwt()
        {
            var validToken =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOiIxNTk5NzUzMTk5IiwiYWNjZXNzX3Rva2VuIjoidGVzdF9hY2Nlc3NfdG9rZW4iLCJpZCI6IjAifQ.osL8kyPx90gUapZzz6Iv-H8DPwgtJTMSKTJA1VtMirU";
            var validExp = _DateTimeProvider.Now().AddHours(_ClaimLifetimeHours - .1).ToUnixTimeU64();
            var testClaims = new Dictionary<string, string>
            {
                {"id", "0"},
                {"exp", validExp.ToString()},
                {"access_token", validToken}
            };

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

            Assert.True(await _JwtClaimValidator.Validate(testClaims));
        }

        
        [Fact]
        public async Task ValidateShouldReturnFalseOnInValidExpiryInJwtPayload()
        {
            var validToken =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOiIxNTk5NzUzMTk5IiwiYWNjZXNzX3Rva2VuIjoidGVzdF9hY2Nlc3NfdG9rZW4iLCJpZCI6IjAifQ.osL8kyPx90gUapZzz6Iv-H8DPwgtJTMSKTJA1VtMirU";
            var inValidExp = _DateTimeProvider.Now().AddHours(_ClaimLifetimeHours + .1).ToUnixTimeU64();
            var testClaims = new Dictionary<string, string>
            {
                {"id", "0"},
                {"exp", inValidExp.ToString()},
                {"access_token", validToken}
            };

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

            Assert.False(await _JwtClaimValidator.Validate(testClaims));
        }
        
        [Fact]
        public async Task ValidateShouldReturnFalseOnInValidTIHJwt()
        {
            var validToken =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOiIxNTk5NzUzMTk5IiwiYWNjZXNzX3Rva2VuIjoidGVzdF9hY2Nlc3NfdG9rZW4iLCJpZCI6IjAifQ.osL8kyPx90gUapZzz6Iv-H8DPwgtJTMSKTJA1VtMirU";
            var validExp = _DateTimeProvider.Now().AddHours(_ClaimLifetimeHours - .1).ToUnixTimeU64();
            var testClaims = new Dictionary<string, string>
            {
                {"id", "0"},
                {"exp", validExp.ToString()},
                {"access_token", validToken + "_im_now_invalid"}
            };

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

            Assert.False(await _JwtClaimValidator.Validate(testClaims));
        }
    }
}