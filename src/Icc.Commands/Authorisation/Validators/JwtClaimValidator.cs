// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Validators
{
    public class JwtClaimValidator : IJwtClaimValidator
    {
        private readonly ITheIdentityHubService _TheIdentityHubService;
        private readonly ILogger<JwtClaimValidator> _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IIccPortalConfig _IccPortalConfig;

        public JwtClaimValidator(ITheIdentityHubService theIdentityHubService, ILogger<JwtClaimValidator> logger,
            IUtcDateTimeProvider dateTimeProvider, IIccPortalConfig iccPortalConfig)
        {
            _TheIdentityHubService =
                theIdentityHubService ?? throw new ArgumentNullException(nameof(theIdentityHubService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _IccPortalConfig = iccPortalConfig ?? throw new ArgumentNullException(nameof(iccPortalConfig));
        }

        public async Task<bool> ValidateAsync(IDictionary<string, string> decodedClaims)
        {
            if (decodedClaims == null) throw new ArgumentNullException(nameof(decodedClaims));

            if (!decodedClaims.ContainsKey("exp") || !decodedClaims.ContainsKey("access_token"))
            {
                return false;
            }
            // check exp max. 3 hrs
            
            var maxExpiry = _DateTimeProvider.Snapshot.AddHours(_IccPortalConfig.ClaimLifetimeHours).ToUnixTimeU64();

            if (!ulong.TryParse(decodedClaims["exp"], out var tokenExpiry))
            {
                return false;
            }

            if (tokenExpiry > maxExpiry)
            {
                _Logger.WriteTokenExpTooLong(_IccPortalConfig.ClaimLifetimeHours.ToString());
                return false;
            }
            
            return await _TheIdentityHubService.VerifyTokenAsync(decodedClaims["access_token"]);
        }
    }
}