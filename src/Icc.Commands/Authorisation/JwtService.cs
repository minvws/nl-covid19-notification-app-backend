// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class JwtService : IJwtService
    {
        private readonly IIccPortalConfig _IccPortalConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ILogger _Logger;

        public JwtService(IIccPortalConfig iccPortalConfig, IUtcDateTimeProvider dateTimeProvider, ILogger<JwtService> logger)
        {
            _IccPortalConfig = iccPortalConfig ?? throw new ArgumentNullException(nameof(iccPortalConfig));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private JwtBuilder CreateBuilder()
        {
            return new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(_IccPortalConfig.JwtSecret)
                .MustVerifySignature();
        }

        public string Generate(ulong exp, Dictionary<string, object> claims)
        {
            if (claims == null) throw new ArgumentNullException(nameof(claims));
            // any further validation of claims?

            var builder = CreateBuilder();
            builder.AddClaim("exp", exp.ToString());

            foreach (var (key, value) in claims)
            {
                builder.AddClaim(key, value);
            }

            return builder.Encode();
        }

        public string Generate(IList<AuthClaim> claimsPrincipal)
        {
            if (claimsPrincipal == null)
                throw new ArgumentNullException(nameof(claimsPrincipal));

            var builder = CreateBuilder();
            builder.AddClaim("exp",
                _DateTimeProvider.Snapshot.AddHours(_IccPortalConfig.ClaimLifetimeHours).ToUnixTimeU64());
            builder.AddClaim("id", GetClaimValue(claimsPrincipal, ClaimTypes.NameIdentifier));
            builder.AddClaim("access_token",
                GetClaimValue(claimsPrincipal, TheIdentityHubClaimTypes.AccessToken));
            builder.AddClaim("name",
                GetClaimValue(claimsPrincipal, TheIdentityHubClaimTypes.DisplayName));
            return builder.Encode();
        }

        private string? GetClaimValue(IList<AuthClaim> claimList, string claimType) =>
            claimList.FirstOrDefault(c => c.Type.Equals(claimType))?.Value;


        public bool TryDecode(string token, out IDictionary<string, string> payload)
        {
            payload = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                payload = CreateBuilder().Decode<IDictionary<string, object>>(token)
                    .ToDictionary(x => x.Key, x => x.Value.ToString());

                return true;
            }
            catch (FormatException exception)
            {
                _Logger.WriteInvalidTokenFormat(exception);
            }
            catch (InvalidTokenPartsException exception)
            {
                _Logger.WriteInvalidTokenParts(exception);
            }
            catch (TokenExpiredException exception)
            {
                _Logger.WriteTokenExpired(exception);
            }
            catch (SignatureVerificationException exception)
            {
                _Logger.WriteTokenSigInvalid(exception);
            }
            catch (Exception exception)
            {
                _Logger.WriteTokenOtherError(exception);
            }

            return false;
        }
    }
}