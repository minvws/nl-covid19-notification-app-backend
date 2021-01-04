// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    /// <summary>
    /// Additional metadata for inbound filter processing the contents of a received DKS
    /// </summary>
    public class DkProcessingItem
    {
        public DiagnosisKeyEntity DiagnosisKey { get; set; }

        /// <summary>
        /// TODO if this is not required by any of the inbound filters, it can be removed.
        /// </summary>
        public IDictionary<string, object> Metadata { get; set; }
    }
}