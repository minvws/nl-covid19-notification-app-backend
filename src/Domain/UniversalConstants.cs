// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    /// <summary>
    /// This should replace certain config values as it is highly unlikely they will ever change.
    /// </summary>
    public class UniversalConstants
    {
        public const int DailyKeyDataLength = 16;
        
        [Obsolete("RollingPeriodRange.Hi")]
        public const int RollingPeriodMax = 144;

        public static readonly Range<int> RollingPeriodRange = new Range<int>(1, 144);

    }
}