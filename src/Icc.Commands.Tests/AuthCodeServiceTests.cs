// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components.Tests
{
    public class AuthCodeServiceTests
    {
        private IAuthCodeService _AuthCodeService;
        private IAuthCodeGenerator _AuthCodeGenerator;

        public AuthCodeServiceTests()
        {
            var randomNumberGenerator = new StandardRandomNumberGenerator();
            _AuthCodeGenerator = new AuthCodeGenerator(randomNumberGenerator);
            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

            _AuthCodeService = new AuthCodeService(cache, _AuthCodeGenerator);
        }

        [InlineData(32)]
        [InlineData(12)]
        [InlineData(1)]
        [InlineData(64)]
        [Theory]
        public void GeneratorGeneratesXLengthString(int expectedLength)
        {
            var authCode = _AuthCodeGenerator.Next(expectedLength);

            Assert.NotNull(authCode);
            Assert.True(authCode.Length == expectedLength);
        }

        [Fact]
        public async void ValidateIfGeneratedAuthCodeIsStored()
        {
            var authCode = await _AuthCodeService.GenerateAuthCodeAsync(new ClaimsPrincipal());

            var result = await _AuthCodeService.GetClaimsByAuthCodeAsync(authCode);

            Assert.NotNull(result);
        }


        [Fact]
        public async void ValidateIfClaimsPrincipalIsStored()
        {
            var testClaim = new Claim("testType", "testValue", "string");
            var identity = new ClaimsIdentity(new List<Claim>() {testClaim}, "test");

            var expectedClaimsPrincipal = new ClaimsPrincipal(identity);

            var authCode = await _AuthCodeService.GenerateAuthCodeAsync(expectedClaimsPrincipal);

            var outputClaims = await _AuthCodeService.GetClaimsByAuthCodeAsync(authCode);

            Assert.Equal(testClaim.Type, outputClaims?[0].Type);
            Assert.Equal(testClaim.Value, outputClaims?[0].Value);
        }

        [Fact]
        public async Task ValidateIfAuthCodeIsRevokedCorrectly()
        {
            var authCode = await _AuthCodeService.GenerateAuthCodeAsync(new ClaimsPrincipal());

            await _AuthCodeService.RevokeAuthCodeAsync(authCode);

            var result = await _AuthCodeService.GetClaimsByAuthCodeAsync(authCode);

            Assert.Null(result);
        }
    }
}