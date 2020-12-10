// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class IksEngineResult
    {
        public DateTime Started { get; set; }
        public double SnapshotSeconds { get; set; }
        public double TotalSeconds { get; set; }
        public int InputCount { get; set; }
        public int OutputCount { get; set; }
        public IksInfo[] Items { get; set; }
        public int ReconcileOutputCount => InputCount - OutputCount;
        public int ReconcileEksSumCount => Items.Sum(x => x.ItemCount) - OutputCount;
    }
}