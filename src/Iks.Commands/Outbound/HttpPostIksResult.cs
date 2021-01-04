// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class HttpPostIksResult
    {
        public HttpStatusCode? HttpResponseCode { get; set; }
        public bool Exception { get; set; }
        public string? Content { get; set; }
    }
}