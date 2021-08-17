// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public class HttpGetIksResult
    {
        //Id for the first batch of confirmation of the request
        public string BatchTag { get; set; }
        public string NextBatchTag { get; set; }
        public byte[] Content { get; set; }
        public DateTime RequestedDay { get; set; }
        public HttpStatusCode ResultCode { get; set;}
    }
}
