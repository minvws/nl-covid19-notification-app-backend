// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Tests
{
    public class ByteArrayEqualityComparerTests
    {
        private readonly byte[] x = { 1, 2, 3 };
        private readonly byte[] y = { 1, 2, 3 };
        private readonly byte[] z = { 4, 5, 6 };

        private readonly ByteArrayEqualityComparer comparer = new ByteArrayEqualityComparer();

        [Fact]
        public void Equal_Content_Should_Be_Equal()
        {
            Assert.True(comparer.Equals(x, y));
        }

        [Fact]
        public void Equal_Content_Should_Have_Same_HashCode()
        {
            Assert.Equal(comparer.GetHashCode(x), comparer.GetHashCode(y));
        }

        [Fact]
        public void Different_Content_Should_Be_Unequal()
        {
            Assert.False(comparer.Equals(y, z));   
        }

        [Fact]
        public void Different_Content_Should_Have_Different_HashCode()
        {
            Assert.NotEqual(comparer.GetHashCode(y), comparer.GetHashCode(z));
        }
    }
}