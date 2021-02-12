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
        public const int DailyKeyDataByteCount = 16;

        public static readonly Range<int> RollingPeriodRange = new Range<int>(1, 144);

        public const int BucketIdByteCount = 32;
        public const int ConfirmationKeyByteCount = 32;
        public const int PostKeysSignatureByteCount = 32;
    }
}