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
        private readonly ITheIdentityHubService _theIdentityHubService;
        private readonly ILogger<TestJwtClaimValidator> _logger;
        const string TestAccessToken = "test_access_token";

        public TestJwtClaimValidator(ITheIdentityHubService theIdentityHubService,
            ILogger<TestJwtClaimValidator> logger)
        {
            _theIdentityHubService =
                theIdentityHubService ?? throw new ArgumentNullException(nameof(theIdentityHubService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ValidateAsync(IDictionary<string, string> decodedClaims)
        {
            if (decodedClaims == null)
                throw new ArgumentNullException(nameof(decodedClaims));

            if (!decodedClaims.ContainsKey("access_token"))
            {
                return false;
            }

            if (decodedClaims["access_token"] == TestAccessToken)
            {
                _logger.WriteTestJwtUsed();
                return true;
            }

            return await _theIdentityHubService.VerifyTokenAsync(decodedClaims["access_token"]);
        }
    }
}
