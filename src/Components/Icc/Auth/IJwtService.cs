// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Auth
{
    public interface IJwtService
    {
        string Generate(ulong exp, Dictionary<string, object> claims);
        string Generate(IList<AuthClaim> authClaim);
        
        bool TryDecode(string token, out IDictionary<string, string> payload);
    }
}