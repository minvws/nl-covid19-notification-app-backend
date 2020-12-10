// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public struct Range<T> : IEquatable<Range<T>>
        where T : struct, IComparable<T>, IEquatable<T>
    {
        //[JsonConstructor]
        public Range(T lo, T hi)
        {
            if (lo.CompareTo(hi) > 0)
                throw new ArgumentOutOfRangeException(nameof(hi), "lo > hi");

            Lo = lo;
            Hi = hi;
        }

        //Private is hack for PSerializable
        public T Lo { get; private set; }
        public T Hi { get; private set; }

        public override string ToString()
        {
            return $"{Lo},{Hi}";
        }

        public bool Equals(Range<T> other)
        {
            return Lo.CompareTo(other.Lo) == 0 && Hi.CompareTo(other.Hi) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Range<T> range && Equals(range);
        }

        public override int GetHashCode()
        {
            unchecked { return (Lo.GetHashCode() * 397) ^ Hi.GetHashCode(); }
        }

        public static bool operator ==(Range<T> left, Range<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Range<T> left, Range<T> right)
        {
            return !left.Equals(right);
        }

        public Range(T lo)
        {
            Lo = lo;
            Hi = lo;
        }

        public bool Contains(T value)
        {
            return Lo.CompareTo(value) <= 0 && value.CompareTo(Hi) <= 0;
        }

        public bool Contains(Range<T> rhs)
        {
            return Contains(rhs.Lo) && Contains(rhs.Hi);
        }

        public Range<T> CombineWide(Range<T> other)
        {
            var lo = Lo.CompareTo(other.Lo) <= 0 ? Lo : other.Lo;
            var hi = Hi.CompareTo(other.Hi) >= 0 ? Hi : other.Hi;
            return new Range<T>(lo, hi);
        }

        public Range<T> CombineNarrow(Range<T> other)
        {
            var lo = Lo.CompareTo(other.Lo) >= 0 ? Lo : other.Lo;
            var hi = Hi.CompareTo(other.Hi) <= 0 ? Hi : other.Hi;
            return new Range<T>(lo, hi);
        }

        public T Clamp(T value)
        {
            ////Check.Require(lo.CompareTo(hi) <= 0);
            if (value.CompareTo(Lo) < 0)
                return Lo;
            if (value.CompareTo(Hi) > 0)
                return Hi;

            return value;
        }
    }
}