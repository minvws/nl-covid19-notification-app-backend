// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class RemoveExpiredManifestsCommandResult
    {
        public int Found { get; set; }
        public int WalkingDead { get; set; }
        public int Killed { get; set; }
        public int Remaining { get; set; }

        public int Reconciliation => Found - Killed - Remaining;
        public int DeadReconciliation => WalkingDead - Killed;
    }
}