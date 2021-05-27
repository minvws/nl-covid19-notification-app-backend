// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class EksEngineResult
    {
        public DateTime Started { get; set; }
        public double SnapshotSeconds { get; set; }
        public double TotalSeconds { get; set; }
        public int InputCount { get; set; }
        public int FilteredInputCount { get; set; }
        public int TransmissionRiskNoneCount { get; set; }
        public int StuffingCount { get; set; }
        public int OutputCount { get; set; }
        public EksInfo[] EksInfo { get; set; }
        public int ReconcileOutputCount => InputCount + StuffingCount - TransmissionRiskNoneCount - OutputCount;
        public int ReconcileEksSumCount => EksInfo.Sum(x => x.TekCount) - OutputCount;

        //public int DupesFound { get; set; } ?
    }
}