// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Validators
{
    public class TestJwtClaimValidator : IJwtClaimValidator
    {
        private readonly ITheIdentityHubService _TheIdentityHubService;
        private readonly ILogger<TestJwtClaimValidator> _Logger;
        const string _TestAccessToken = "test_access_token";

        public TestJwtClaimValidator(ITheIdentityHubService theIdentityHubService,
            ILogger<TestJwtClaimValidator> logger, IJwtService? jwtService)
        {
            _TheIdentityHubService =
                theIdentityHubService ?? throw new ArgumentNullException(nameof(theIdentityHubService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ValidateAsync(IDictionary<string, string> decodedClaims)
        {
            if (decodedClaims == null) throw new ArgumentNullException(nameof(decodedClaims));

            if (!decodedClaims.ContainsKey("access_token"))
            {
                return false;
            }

            if (decodedClaims["access_token"] == _TestAccessToken)
            {
                _Logger.WriteTestJwtUsed();
                return true;
            }

            return await _TheIdentityHubService.VerifyTokenAsync(decodedClaims["access_token"]);
        }
    }
}