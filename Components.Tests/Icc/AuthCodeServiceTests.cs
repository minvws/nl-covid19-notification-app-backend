// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Icc
{
    [TestClass]
    public class AuthCodeServiceTests
    {
        private IAuthCodeService _AuthCodeService;

        [TestInitialize]
        public void Initialize()
        {
            var cryptoRandomPaddingGenerator = new CryptoRandomPaddingGenerator(new StandardRandomNumberGenerator());
            var cache = new Mock<IDistributedCache>();
            var protectionProvider = DataProtectionProvider.Create(nameof(AuthCodeServiceTests));
            
            _AuthCodeService = new AuthCodeService(cryptoRandomPaddingGenerator, cache.Object, protectionProvider);
        }

        [TestMethod]
        public async Task ValidateIfGeneratedAuthCodeIsStoredAsync()
        {
            var authCode = await _AuthCodeService.GenerateAuthCodeAsync(new ClaimsPrincipal());

            var result = await _AuthCodeService.GetClaimsByAuthCodeAsync(authCode);

            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task ValidateIfClaimsPrincipalIsStoredAsync()
        {
            var identity = new ClaimsIdentity(null, TheIdentityHubDefaults.AuthenticationScheme);
            var expectedClaimsPrincipal = new ClaimsPrincipal(identity);

            var authCode = await _AuthCodeService.GenerateAuthCodeAsync(expectedClaimsPrincipal);

            var outputClaims = await _AuthCodeService.GetClaimsByAuthCodeAsync(authCode);

            Assert.AreEqual(expectedClaimsPrincipal, outputClaims);
        }

        [TestMethod]
        public async Task ValidateIfAuthCodeIsRevokedCorrectlyAsync()
        {
            var authCode = await _AuthCodeService.GenerateAuthCodeAsync(new ClaimsPrincipal());

            await _AuthCodeService.RevokeAuthCodeAsync(authCode);

            var result = await _AuthCodeService.GetClaimsByAuthCodeAsync(authCode);

            Assert.IsNull(result);
        }
    }
}