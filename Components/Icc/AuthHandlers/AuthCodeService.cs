// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public class AuthClaim
    {
        public readonly string? Type;
        public readonly string? Value;

        public AuthClaim()
        {
            
        }
        public AuthClaim(Claim claim)
        {
            if (claim.Type != null) Type = claim.Type;
            if (claim.Value != null) Value = claim.Value;
        }
    }
    public class AuthCodeService : IAuthCodeService
    {
        private readonly IPaddingGenerator _RandomGenerator;
        private readonly IDistributedCache _cache;
        private readonly IDataProtector _protector;

        public AuthCodeService(IPaddingGenerator randomGenerator, IDistributedCache cache, IDataProtectionProvider dataProtectionProvider)
        {
            _RandomGenerator = randomGenerator ?? throw new ArgumentNullException(nameof(randomGenerator));
            _cache =  cache ?? throw new ArgumentNullException(nameof(cache));
            _protector = dataProtectionProvider?.CreateProtector(typeof(IDistributedCache).FullName) ?? throw new ArgumentNullException(nameof(dataProtectionProvider));
        }


        public async Task<string> GenerateAuthCodeAsync(ClaimsPrincipal claimsPrincipal)
        {
            if(claimsPrincipal == null) throw new ArgumentNullException(nameof(claimsPrincipal));

            var authCode = _RandomGenerator.Generate(12);  // TODO: Add url friendly generator

            
            List<AuthClaim> claimsObject = claimsPrincipal.Claims.Select(claim => new AuthClaim(claim)).ToList();
            
            var principalJson = JsonConvert.SerializeObject(claimsObject);
            var encodedClaimsPrincipal = Encoding.UTF8.GetBytes(principalJson);

            //TODO: add sliding expiration
            await _cache.SetAsync(authCode, _protector.Protect(encodedClaimsPrincipal));

            return authCode;
        }

        public async Task<List<AuthClaim>> GetClaimsByAuthCodeAsync(string authCode)
        {
            List<AuthClaim> result = null;

            var encodedClaimsPrincipal = await _cache.GetAsync(authCode);

            if(encodedClaimsPrincipal != null)
            {
                var principalJson = Encoding.UTF8.GetString(_protector.Unprotect(encodedClaimsPrincipal));
                result = JsonConvert.DeserializeObject<List<AuthClaim>>(principalJson); // throws null ref.e.
                return result;
            }

            return result;
        }

        public async Task RevokeAuthCodeAsync(string authCode)
        {
            await _cache.RemoveAsync(authCode);
        }
    }
}