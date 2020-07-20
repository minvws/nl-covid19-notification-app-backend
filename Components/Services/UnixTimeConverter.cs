// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services
{
    //TODO convert DateTimes to DateTimeOffsets?
    public static class UnixTimeConverter
    {
        public static DateTime FromUnixTime(this long value)
        {
            if (value < 0)
                throw new ArgumentException(nameof(value));

            return DateTimeOffset.UnixEpoch.AddSeconds(value).DateTime;
        }

        public static ulong ToUnixTime(this DateTime value)
        {
            return Convert.ToUInt64(Math.Floor((value - DateTimeOffset.UnixEpoch).TotalSeconds));
        }
    }
}
