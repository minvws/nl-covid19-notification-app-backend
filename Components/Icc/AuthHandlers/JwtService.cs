// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using JWT.Algorithms;
using JWT.Builder;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.IccBackend;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public class JwtService : IJwtService
    {
        private readonly IIccPortalConfig _IccPortalConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public JwtService(IIccPortalConfig iccPortalConfig, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _IccPortalConfig = iccPortalConfig ?? throw new ArgumentNullException(nameof(iccPortalConfig));
            _DateTimeProvider = utcDateTimeProvider;
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
            //TODO nay validation on exp?
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

        public string Generate(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                throw new ArgumentNullException(nameof(claimsPrincipal));

            var builder = CreateBuilder();
            builder.AddClaim("exp", _DateTimeProvider.Now().AddHours(_IccPortalConfig.ClaimLifetimeHours).ToUnixTime());
            builder.AddClaim("id", GetClaimValue(claimsPrincipal, ClaimTypes.NameIdentifier));
            builder.AddClaim("email", GetClaimValue(claimsPrincipal, ClaimTypes.Email));
            builder.AddClaim("access_token", GetClaimValue(claimsPrincipal, "http://schemas.u2uconsult.com/ws/2014/03/identity/claims/accesstoken"));
            builder.AddClaim("name", GetClaimValue(claimsPrincipal, "http://schemas.u2uconsult.com/ws/2014/04/identity/claims/displayname"));
            return builder.Encode();
        }

        private string? GetClaimValue(ClaimsPrincipal cp, string claimType) => cp.Claims.FirstOrDefault(c => c.Type.Equals(claimType))?.Value;

        public bool IsValid(IDictionary<string, string> tokens, string checkElement)
        {
            if (string.IsNullOrWhiteSpace(checkElement))
                throw new ArgumentException(nameof(checkElement));

            if (tokens == null)
                return false;

            return tokens.TryGetValue(checkElement, out var checkElementValue) && !string.IsNullOrWhiteSpace(checkElementValue);
        }

        public IDictionary<string, string> Decode(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException(nameof(token));

            return CreateBuilder().Decode<IDictionary<string, object>>(token).ToDictionary(x => x.Key, x=> x.Value.ToString());
        }
    }
}