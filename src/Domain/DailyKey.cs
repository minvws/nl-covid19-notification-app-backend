// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    [Owned]
    public class DailyKey
    {
        public DailyKey()
        {
        }

        public DailyKey(byte[] keyData, int rollingStartNumber, int rollingPeriod)
        {
            KeyData = keyData ?? throw new ArgumentNullException(nameof(keyData));
            RollingStartNumber = rollingStartNumber;
            RollingPeriod = rollingPeriod;
        }

        public byte[] KeyData { get; set; }
        public int RollingStartNumber { get; set; }
        public int RollingPeriod { get; set; }
    }
}
