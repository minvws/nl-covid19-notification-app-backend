// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services
{

    /// <summary>
    /// TODO has the definition of Epoch changed yet again?
    /// </summary>
    public static class UnixTimeConverter
    {
        public static readonly DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(this long value)
        {
            if (value < 0)
                throw new ArgumentException(nameof(value));

            return EpochStart.AddSeconds(value);
        }

        public static ulong ToUnixTime(this DateTime value)
        {
            if (value < EpochStart)
                throw new ArgumentException(nameof(value));

            return Convert.ToUInt64(Math.Floor((value - EpochStart).TotalSeconds));
        }
    }
}
