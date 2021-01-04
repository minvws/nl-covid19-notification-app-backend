// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        private static readonly EqualityComparer<byte> ElementComparer = EqualityComparer<byte>.Default;

        private static bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        public bool Equals(byte[] x, byte[] y)
        {
            return ByteArrayCompare(x, y);
        }

        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                return obj == null
                    ? 0
                    : obj.Aggregate(
                        17,
                        (current, element) => current * 31 + ElementComparer.GetHashCode(element));
            }
        }
    }
}