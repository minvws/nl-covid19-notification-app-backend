// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Security.Claims;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public class AuthCodeService
    {
        private readonly ConcurrentDictionary<string, ClaimsPrincipal> _AuthCodeStorage;
        private readonly IPaddingGenerator _RandomGenerator;

        public AuthCodeService(IPaddingGenerator randomGenerator)
        {
            _RandomGenerator = randomGenerator ?? throw new ArgumentNullException(nameof(randomGenerator));
            _AuthCodeStorage = new ConcurrentDictionary<string, ClaimsPrincipal>();
        }
        
        public string Generate(ClaimsPrincipal claimsPrincipal)
        {
            var authCode = _RandomGenerator.Generate(64);
            _AuthCodeStorage.AddOrUpdate(authCode, claimsPrincipal, (s, principal) => principal);

            return authCode;
        }

        public bool TryGetClaims(string authCode, out ClaimsPrincipal claimsPrincipal)
        {
            if (!_AuthCodeStorage.TryGetValue(authCode, out claimsPrincipal))
            {
                return false;
            }

            _AuthCodeStorage.TryRemove(authCode, out _);
                
            return true;
        }
    }
}