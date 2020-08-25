// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using System.Security.Claims;
using TheIdentityHub.AspNetCore.Authentication;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Icc
{
    public class AuthCodeServiceTests
    {
        private IAuthCodeService _AuthCodeService;

        public AuthCodeServiceTests()
        {
            var cryptoRandomPaddingGenerator = new CryptoRandomPaddingGenerator(new StandardRandomNumberGenerator());
            _AuthCodeService = new AuthCodeService(cryptoRandomPaddingGenerator);
        }

        [Fact]
        public void ValidateIfGeneratedAuthCodeIsStored()
        {
            var authCode = _AuthCodeService.Generate(new ClaimsPrincipal());

            var result = _AuthCodeService.TryGetClaims(authCode, out _);

            Assert.True(result);
        }


        [Fact]
        public void ValidateIfClaimsPrincipalIsStored()
        {
            var identity = new ClaimsIdentity(null, TheIdentityHubDefaults.AuthenticationScheme);
            var expectedClaimsPrincipal = new ClaimsPrincipal(identity);

            var authCode = _AuthCodeService.Generate(expectedClaimsPrincipal);

            var successful = _AuthCodeService.TryGetClaims(authCode, out var outputClaims);

            Assert.True(successful);
            Assert.Equal(expectedClaimsPrincipal, outputClaims);
        }

        [Fact]
        public void ValidateIfAuthCodeIsRevokedCorrectly()
        {
            var authCode = _AuthCodeService.Generate(new ClaimsPrincipal());

            _AuthCodeService.RevokeAuthCode(authCode);

            var result = _AuthCodeService.TryGetClaims(authCode, out _);

            Assert.False(result);
        }
    }
}