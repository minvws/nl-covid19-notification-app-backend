// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    [Owned]
    public class DailyKey //: IEquatable<DailyKey>
    {
        public DailyKey()
        {
        }

        //TODO Reinstate when this can be made a struct.
        //public bool Equals(DailyKey? other)
        //{
        //    if (ReferenceEquals(null, other)) return false;
        //    if (ReferenceEquals(this, other)) return true;
        //    return KeyData.Equals(other.KeyData) && RollingStartNumber == other.RollingStartNumber && RollingPeriod == other.RollingPeriod;
        //}

        //public override bool Equals(object? obj)
        //{
        //    if (ReferenceEquals(null, obj)) return false;
        //    if (ReferenceEquals(this, obj)) return true;
        //    if (obj.GetType() != this.GetType()) return false;
        //    return Equals((DailyKey) obj);
        //}

        //public override int GetHashCode()
        //{
        //    return HashCode.Combine(KeyData, RollingStartNumber, RollingPeriod);
        //}

        //public static bool operator ==(DailyKey? left, DailyKey? right)
        //{
        //    return Equals(left, right);
        //}

        //public static bool operator !=(DailyKey? left, DailyKey? right)
        //{
        //    return !Equals(left, right);
        //}

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