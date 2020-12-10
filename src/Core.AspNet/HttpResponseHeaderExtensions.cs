// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Net.Http.Headers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public static class HttpResponseHeaderExtensions
    {
        public static string SafeGetValue(this HttpResponseHeaders headers, string header)
        {
            return headers.Contains(header)
                ? headers.GetValues(header).First()
                : string.Empty;
        }
    }
}