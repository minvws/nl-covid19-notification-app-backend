// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code
{
    public class AuthCodeService : IAuthCodeService
    {
        private readonly IDistributedCache _cache;
        private readonly IAuthCodeGenerator _authCodeGenerator;

        public AuthCodeService(IDistributedCache cache,
            IAuthCodeGenerator authCodeGenerator)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _authCodeGenerator = authCodeGenerator ?? throw new ArgumentNullException(nameof(authCodeGenerator));
        }

        public async Task<string> GenerateAuthCodeAsync(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var authCode = _authCodeGenerator.Next();

            var claimsObject = claimsPrincipal.Claims.Select(claim => new AuthClaim(claim.Type, claim.Value)).ToList();

            var principalJson = JsonConvert.SerializeObject(claimsObject);
            var encodedClaimsPrincipal = Encoding.UTF8.GetBytes(principalJson);

            //TODO: add sliding expiration
            await _cache.SetAsync(authCode, encodedClaimsPrincipal);

            return authCode;
        }

        public async Task<List<AuthClaim>> GetClaimsByAuthCodeAsync(string authCode)
        {
            if (string.IsNullOrWhiteSpace(authCode))
            {
                throw new ArgumentException(nameof(authCode));
            }

            List<AuthClaim> result = null;

            var encodedAuthClaimList = await _cache.GetAsync(authCode);

            if (encodedAuthClaimList != null)
            {
                var claimListJson = Encoding.UTF8.GetString(encodedAuthClaimList);
                result = JsonConvert.DeserializeObject<List<AuthClaim>>(claimListJson);
            }

            return result;
        }

        public async Task RevokeAuthCodeAsync(string authCode)
        {
            if (string.IsNullOrWhiteSpace(authCode))
            {
                throw new ArgumentException(nameof(authCode));
            }

            await _cache.RemoveAsync(authCode);
        }
    }
}
