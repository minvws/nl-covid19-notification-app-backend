// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Tests
{
    public class ByteArrayEqualityComparerTests
    {
        private readonly byte[] _x = { 1, 2, 3 };
        private readonly byte[] _y = { 1, 2, 3 };
        private readonly byte[] _z = { 4, 5, 6 };

        private readonly ByteArrayEqualityComparer _comparer = new ByteArrayEqualityComparer();

        [Fact]
        public void Equal_Content_Should_Be_Equal()
        {
            Assert.True(_comparer.Equals(_x, _y));
        }

        [Fact]
        public void Equal_Content_Should_Have_Same_HashCode()
        {
            Assert.Equal(_comparer.GetHashCode(_x), _comparer.GetHashCode(_y));
        }

        [Fact]
        public void Different_Content_Should_Be_Unequal()
        {
            Assert.False(_comparer.Equals(_y, _z));
        }

        [Fact]
        public void Different_Content_Should_Have_Different_HashCode()
        {
            Assert.NotEqual(_comparer.GetHashCode(_y), _comparer.GetHashCode(_z));
        }
    }
}