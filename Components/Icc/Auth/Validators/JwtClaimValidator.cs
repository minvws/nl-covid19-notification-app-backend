// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JWT;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Config;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
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

        public async Task<bool> Validate(IDictionary<string, string> decodedClaims)
        {
            if (decodedClaims == null) throw new ArgumentNullException(nameof(decodedClaims));

            if (!decodedClaims.ContainsKey("exp") || !decodedClaims.ContainsKey("access_token"))
            {
                return false;
            }
            // check exp max. 3 hrs
            
            var maxExpiry = _DateTimeProvider.Snapshot.AddHours(_IccPortalConfig.ClaimLifetimeHours).ToUnixTimeU64();

            if (!ulong.TryParse(decodedClaims["exp"], out ulong tokenExpiry))
            {
                return false;
            }

            if (tokenExpiry > maxExpiry)
            {
                _Logger.LogInformation("Token invalid, has longer exp. than configured {claimLifetimeHours} hrs", _IccPortalConfig.ClaimLifetimeHours.ToString());
                return false;
            }
            
            return await _TheIdentityHubService.VerifyToken(decodedClaims["access_token"]);
        }
    }
}