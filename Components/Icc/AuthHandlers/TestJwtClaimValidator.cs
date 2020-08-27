// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public class TestJwtClaimValidator : IJwtClaimValidator
    {

        private readonly ITheIdentityHubService _TheIdentityHubService;
        private readonly ILogger<TestJwtClaimValidator> _Logger;

        public TestJwtClaimValidator(ITheIdentityHubService theIdentityHubService,
            ILogger<TestJwtClaimValidator> logger)
        {
            _TheIdentityHubService =
                theIdentityHubService ?? throw new ArgumentNullException(nameof(theIdentityHubService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        async Task<bool> IJwtClaimValidator.Validate(IDictionary<string, string> decodedClaims)
        {
            if (decodedClaims["access_token"] == "test_access_token")
            {
                _Logger.LogWarning("Test JWT Used for authorization!");
                return true;
            }

            return await _TheIdentityHubService.VerifyToken(decodedClaims["access_token"]);
        }
    }
}