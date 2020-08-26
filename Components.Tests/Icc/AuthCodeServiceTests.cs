// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            _AuthCodeService = new AuthCodeService(cryptoRandomPaddingGenerator);
        }

        [TestMethod]
        public void ValidateIfGeneratedAuthCodeIsStored()
        {
            var authCode = _AuthCodeService.Generate(new ClaimsPrincipal());

            var result = _AuthCodeService.TryGetClaims(authCode, out _);

            Assert.IsTrue(result);
        }


        [TestMethod]
        public void ValidateIfClaimsPrincipalIsStored()
        {
            var identity = new ClaimsIdentity(null, TheIdentityHubDefaults.AuthenticationScheme);
            var expectedClaimsPrincipal = new ClaimsPrincipal(identity);

            var authCode = _AuthCodeService.Generate(expectedClaimsPrincipal);

            var successful = _AuthCodeService.TryGetClaims(authCode, out var outputClaims);

            Assert.IsTrue(successful);
            Assert.AreEqual(expectedClaimsPrincipal, outputClaims);
        }

        [TestMethod]
        public void ValidateIfAuthCodeIsRevokedCorrectly()
        {
            var authCode = _AuthCodeService.Generate(new ClaimsPrincipal());

            _AuthCodeService.RevokeAuthCode(authCode);

            var result = _AuthCodeService.TryGetClaims(authCode, out _);

            Assert.IsFalse(result);
        }
    }
}