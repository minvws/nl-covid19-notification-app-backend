// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services
{
    //TODO convert DateTimes to DateTimeOffsets?
    public static class TimeConverter
    {
        private static readonly DateTime _RollingPeriodEpoch = DateTime.UnixEpoch;
        private const int RollingPeriodFactor = 600; //TODO Currently 1 : ? seconds?

        public static int ToRollingStartNumber(this DateTime value)
        {
            var dif = value - _RollingPeriodEpoch;
            return Convert.ToInt32(Math.Floor(dif.TotalSeconds)) / RollingPeriodFactor;
        }

        //public static long ToRollingPeriodNumber(this DateTime value)
        //{
        //    return Convert.ToInt64(Math.Floor((value - _RollingPeriodEpoch).TotalSeconds) / RollingPeriodFactor);
        //}

        //public static long ToRollingPeriodNumber(this TimeSpan value)
        //{
        //    return Convert.ToInt64(Math.Floor(value.TotalSeconds)) / RollingPeriodFactor;
        //}

        public static DateTime FromRollingStartNumber(this int value)
        {
            return _RollingPeriodEpoch + TimeSpan.FromSeconds(value * RollingPeriodFactor);
        }

        public static TimeSpan FromRollingPeriod(this int value)
        {
            return TimeSpan.FromSeconds(value * RollingPeriodFactor);
        }

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
