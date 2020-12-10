// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
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