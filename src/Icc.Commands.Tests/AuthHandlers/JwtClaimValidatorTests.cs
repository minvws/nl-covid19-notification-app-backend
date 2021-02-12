// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Validators;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components.Tests.AuthHandlers
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

            var iccPortalConfigMock = new Mock<IIccPortalConfig>();
            iccPortalConfigMock.Setup(x => x.ClaimLifetimeHours).Returns(_ClaimLifetimeHours);
            iccPortalConfigMock.Setup(x => x.FrontendBaseUrl).Returns("http://test.test");
            iccPortalConfigMock.Setup(x => x.JwtSecret).Returns("test_secret123");
            iccPortalConfigMock.Setup(x => x.StrictRolePolicyEnabled).Returns(true);

            _JwtClaimValidator =
                new JwtClaimValidator(TestTheIdentityHubServiceCreator.CreateInstance(_Server), logger,
                    _DateTimeProvider,
                    iccPortalConfigMock.Object);
        }

        [Fact]
        public async Task ValidateShouldReturnTrueOnValidTIHJwt()
        {
            var validToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOiIxNTk5NzUzMTk5IiwiYWNjZXNzX3Rva2VuIjoidGVzdF9hY2Nlc3NfdG9rZW4iLCJpZCI6IjAifQ.osL8kyPx90gUapZzz6Iv-H8DPwgtJTMSKTJA1VtMirU";
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

            Assert.True(await _JwtClaimValidator.ValidateAsync(testClaims));
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

            Assert.False(await _JwtClaimValidator.ValidateAsync(testClaims));
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

            Assert.False(await _JwtClaimValidator.ValidateAsync(testClaims));
        }
    }
}