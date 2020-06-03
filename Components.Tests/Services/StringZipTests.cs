// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System.Text;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Components.Tests.Services
//{
//    [TestClass()]
//    public class StringZipTests
//    {
//        [TestMethod()]
//        public void RoundTrip()
//        {
//            var value = "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. " +
//                        "The quick brown fox jumps over the lazy dog. ";

//            var valueBytes = Encoding.UTF8.GetBytes(value);
//            var zipped = valueBytes.Zip();
//            var unzipped = zipped.Unzip();
//            CollectionAssert.AreEqual(valueBytes, unzipped);
//            var actual = Encoding.UTF8.GetString(unzipped);
//            Assert.AreEqual(value, actual);
//        }
//    }
//}