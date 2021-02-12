// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Validators;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components.Tests.AuthHandlers
{
    public class TestJwtClaimValidatorTests
    {
        private readonly TestJwtClaimValidator _TestJwtClaimValidator;
        private readonly WireMockServer _Server;

        public TestJwtClaimValidatorTests()
        {
            var logger = new TestLogger<TestJwtClaimValidator>();
            _Server = WireMockServer.Start();
            _TestJwtClaimValidator =
                new TestJwtClaimValidator(TestTheIdentityHubServiceCreator.CreateInstance(_Server), logger, null!);
        }

        [Fact]
        public async Task ValidateShouldReturnTrueOnValidTestToken()
        {
            var testClaims = new Dictionary<string, string> {{"id", "0"}, {"access_token", "test_access_token"}};
            // test_access_token

            Assert.True(await _TestJwtClaimValidator.ValidateAsync(testClaims));
        }

        [InlineData("12345")]
        [InlineData(
            "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9")]
        [InlineData("")]
        [Theory]
        public async Task ValidateShouldReturnFalseOnInvalidTestTokens(string testAccessToken)
        {
            var testClaims = new Dictionary<string, string> {{"id", "0"}, {"access_token", testAccessToken}};
            // test_access_token

            Assert.False(await _TestJwtClaimValidator.ValidateAsync(testClaims));
        }

        [Fact]
        public async Task ValidateShouldStillReturnTrueOnValidTIHJwt()
        {
            var validToken =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOiIxNTk5NzUzMTk5IiwiYWNjZXNzX3Rva2VuIjoidGVzdF9hY2Nlc3NfdG9rZW4iLCJpZCI6IjAifQ.osL8kyPx90gUapZzz6Iv-H8DPwgtJTMSKTJA1VtMirU";
            var testClaims = new Dictionary<string, string> {{"id", "0"}, {"access_token", validToken}};

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

            Assert.True(await _TestJwtClaimValidator.ValidateAsync(testClaims));
        }

        [Fact]
        public async Task ValidateShouldStillReturnFalseOnInValidTIHJwt()
        {
            var validToken =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOiIxNTk5NzUzMTk5IiwiYWNjZXNzX3Rva2VuIjoidGVzdF9hY2Nlc3NfdG9rZW4iLCJpZCI6IjAifQ.osL8kyPx90gUapZzz6Iv-H8DPwgtJTMSKTJA1VtMirU";
            var testClaims = new Dictionary<string, string>
                {{"id", "0"}, {"access_token", validToken + "_im_now_invalid"}};

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

            Assert.False(await _TestJwtClaimValidator.ValidateAsync(testClaims));
        }
    }
}