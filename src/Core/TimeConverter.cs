// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public static class TimeConverter
    {
        private const int RollingPeriodFactor = 600;

        /// <summary>
        /// Converts an UTC Date into the RollingStartNumber.
        /// </summary>
        public static int ToRollingStartNumber(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc || value.Date != value)
                throw new ArgumentException("Not UTC or not a date.");

            return value.ToUnixTime() / RollingPeriodFactor;
        }

        public static int DaysSinceSymptomOnset(this int value, DateTime dateOfSymptomsOnset) =>
            Convert.ToInt32(Math.Floor((value.FromRollingStartNumber().Date - dateOfSymptomsOnset).TotalDays));


        public static DateTime FromRollingStartNumber(this int value)
        {
            var epoch = (long)value * RollingPeriodFactor;

            return  DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime;
        }
        
        public static ulong ToUnixTimeU64(this DateTime value)
        {
            return Convert.ToUInt64(value.ToUnixTime());
        }

        private static int ToUnixTime(this DateTime value)
        {
            return Convert.ToInt32(Math.Floor((value - DateTimeOffset.UnixEpoch).TotalSeconds));
        }
    }
}
