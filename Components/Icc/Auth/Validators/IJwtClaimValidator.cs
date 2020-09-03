// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public interface IJwtClaimValidator
    {
        public Task<bool> Validate(IDictionary<string, string> decodedClaims);
    }
}