// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using System.Text;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.WebApi
{
    [TestClass]
    public class CryptoRandomPaddingGeneratorTests
    {   
        [TestMethod]
        public void Generate_returns_different_string_on_each_call()
        {
            // Assemble
            const int length = 64;
            var gen = new CryptoRandomPaddingGenerator(new StandardRandomNumberGenerator());

            // Act
            var resultA = gen.Generate(length);
            var resultB = gen.Generate(length);
            var resultC = gen.Generate(length);

            // Assert
            Assert.AreNotEqual(resultA, resultB);
            Assert.AreNotEqual(resultB, resultC);
            Assert.AreNotEqual(resultA, resultC);
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(42)]
        [DataRow(256)]
        [DataRow(512)]
        [DataRow(1337)]
        public void Generate_returns_string_of_expected_length_in_bytes(int size)
        {
            // Assemble
            var gen = new CryptoRandomPaddingGenerator(new StandardRandomNumberGenerator());

            // Act
            var result = gen.Generate(size);
            var resultByte = Encoding.UTF8.GetBytes(result);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.IsTrue(result.Length == size);
            Assert.IsTrue(result.Length == resultByte.Length);
        }
    }
}
