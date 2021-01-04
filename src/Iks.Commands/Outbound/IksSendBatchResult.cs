// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Net;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class IksSendBatchResult
    {
        public bool Success => Sent.Count(x => x.StatusCode == HttpStatusCode.OK) == Found;
        public int Found { get; set; }
        public IksSendResult[] Sent { get; set; }
    }
}