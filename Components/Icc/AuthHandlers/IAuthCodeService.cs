// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Security.Claims;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public interface IAuthCodeService
    {
        Task<string> GenerateAuthCodeAsync(ClaimsPrincipal claimsPrincipal);

        Task<ClaimsPrincipal?> GetClaimsByAuthCodeAsync(string authCode);

        Task RevokeAuthCodeAsync(string authCode);
    }
}