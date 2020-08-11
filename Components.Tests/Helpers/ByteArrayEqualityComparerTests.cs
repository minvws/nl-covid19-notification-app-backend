// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Helpers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Helpers
{
    [TestClass()]
    public class ByteArrayEqualityComparerTests
    {
        private readonly byte[] x = { 1, 2, 3 };
        private readonly byte[] y = { 1, 2, 3 };
        private readonly byte[] z = { 4, 5, 6 };

        private readonly ByteArrayEqualityComparer comparer = new ByteArrayEqualityComparer();

        [TestMethod()]
        public void Equal_Content_Should_Be_Equal()
        {
            Assert.IsTrue(comparer.Equals(x, y));
        }

        [TestMethod()]
        public void Equal_Content_Should_Have_Same_HashCode()
        {
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));
        }

        [TestMethod()]
        public void Different_Content_Should_Be_Unequal()
        {
            Assert.IsFalse(comparer.Equals(y, z));   
        }

        [TestMethod()]
        public void Different_Content_Should_Have_Different_HashCode()
        {
            Assert.AreNotEqual(comparer.GetHashCode(y), comparer.GetHashCode(z));
        }
    }
}