// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code
{
    public class AuthCodeService : IAuthCodeService
    {
        private readonly IMemoryCache _cache;
        private readonly IAuthCodeGenerator _authCodeGenerator;

        public AuthCodeService(IMemoryCache cache,
            IAuthCodeGenerator authCodeGenerator)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _authCodeGenerator = authCodeGenerator ?? throw new ArgumentNullException(nameof(authCodeGenerator));
        }

        public string GenerateAuthCode(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var authCode = _authCodeGenerator.Next();
            var claims = claimsPrincipal.Claims.Select(claim => new AuthClaim(claim.Type, claim.Value)).ToList();

            _cache.Set(authCode, claims);

            return authCode;
        }

        public List<AuthClaim> GetClaimsByAuthCode(string authCode)
        {
            if (string.IsNullOrWhiteSpace(authCode))
            {
                throw new ArgumentException(nameof(authCode));
            }

            return _cache.Get<List<AuthClaim>>(authCode);
        }

        public void RevokeAuthCode(string authCode)
        {
            if (string.IsNullOrWhiteSpace(authCode))
            {
                throw new ArgumentException(nameof(authCode));
            }

            _cache.Remove(authCode);
        }
    }
}
