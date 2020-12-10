// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public interface ITheIdentityHubService
    {
        public Task<bool> VerifyTokenAsync(string accessToken);
        public Task<bool> RevokeAccessTokenAsync(string accessToken);
        public Task<bool> VerifyClaimTokenAsync(IEnumerable<Claim> userClaims);
    }
}