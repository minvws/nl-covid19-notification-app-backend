// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components.Tests
{
    public class AuthCodeServiceTests
    {
        private readonly IAuthCodeService _authCodeService;
        private readonly IAuthCodeGenerator _authCodeGenerator;

        public AuthCodeServiceTests()
        {
            var randomNumberGenerator = new StandardRandomNumberGenerator();
            _authCodeGenerator = new AuthCodeGenerator(randomNumberGenerator);
            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            _authCodeService = new AuthCodeService(cache, _authCodeGenerator);
        }

        [InlineData(32)]
        [InlineData(12)]
        [InlineData(1)]
        [InlineData(64)]
        [Theory]
        public void GeneratorGeneratesXLengthString(int expectedLength)
        {
            var authCode = _authCodeGenerator.Next(expectedLength);

            Assert.NotNull(authCode);
            Assert.True(authCode.Length == expectedLength);
        }

        [Fact]
        public void ValidateIfGeneratedAuthCodeIsStored()
        {
            var authCode = _authCodeService.GenerateAuthCode(new ClaimsPrincipal());

            var result = _authCodeService.GetClaimsByAuthCode(authCode);

            Assert.NotNull(result);
        }


        [Fact]
        public void ValidateIfClaimsPrincipalIsStored()
        {
            var testClaim = new Claim("testType", "testValue", "string");
            var identity = new ClaimsIdentity(new List<Claim>() { testClaim }, "test");

            var expectedClaimsPrincipal = new ClaimsPrincipal(identity);

            var authCode = _authCodeService.GenerateAuthCode(expectedClaimsPrincipal);

            var outputClaims = _authCodeService.GetClaimsByAuthCode(authCode);

            Assert.Equal(testClaim.Type, outputClaims?[0].Type);
            Assert.Equal(testClaim.Value, outputClaims?[0].Value);
        }

        [Fact]
        public void ValidateIfAuthCodeIsRevokedCorrectly()
        {
            var authCode = _authCodeService.GenerateAuthCode(new ClaimsPrincipal());

            _authCodeService.RevokeAuthCode(authCode);

            var result = _authCodeService.GetClaimsByAuthCode(authCode);

            Assert.Null(result);
        }
    }
}
