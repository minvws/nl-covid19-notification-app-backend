// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{

    public class RemoveExpiredEksCommandResult
    {
        public int Found { get; set; }
        public int Zombies { get; set; }
        public int GivenMercy { get; set; }
        public int Remaining { get; set; }

        public int Reconciliation => Found - GivenMercy - Remaining;
    }
}