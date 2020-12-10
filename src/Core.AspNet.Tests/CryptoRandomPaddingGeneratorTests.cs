// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet.Tests
{
    public class CryptoRandomPaddingGeneratorTests
    {   
        [Fact]
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
            Assert.NotEqual(resultA, resultB);
            Assert.NotEqual(resultB, resultC);
            Assert.NotEqual(resultA, resultC);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(42)]
        [InlineData(256)]
        [InlineData(512)]
        [InlineData(1337)]
        public void Generate_returns_string_of_expected_length_in_bytes(int size)
        {
            // Assemble
            var gen = new CryptoRandomPaddingGenerator(new StandardRandomNumberGenerator());

            // Act
            var result = gen.Generate(size);
            var resultByte = Encoding.UTF8.GetBytes(result);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.True(result.Length == size);
            Assert.True(result.Length == resultByte.Length);
        }
    }
}
