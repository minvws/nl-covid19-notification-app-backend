// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JWT;
using Microsoft.Extensions.Logging;
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

        async Task<bool> IJwtClaimValidator.Validate(IDictionary<string, string> decodedClaims)
        {
            return await _TheIdentityHubService.VerifyToken(decodedClaims["access_token"]);
        }
    }
}