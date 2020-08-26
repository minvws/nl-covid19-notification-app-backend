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
    public interface IAuthCodeService
    {
        public string Generate(ClaimsPrincipal claimsPrincipal);

        public bool TryGetClaims(string authCode, out ClaimsPrincipal claimsPrincipal);

        public bool RevokeAuthCode(string authCode);
    }
}