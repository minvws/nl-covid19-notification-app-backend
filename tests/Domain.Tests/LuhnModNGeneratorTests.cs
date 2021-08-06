// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Tests
{
    public class LuhnModNGeneratorTests
    {
        [Fact]
        public void LuhnModNGenerator_Generates_Valid_Key()
        {
            //Arrange
            var charSet = "BCFGJLQRSTUVXYZ23456789";
            var keyLength = 7;

            var generator = new LuhnModNGenerator(new LuhnModNConfig(charSet, keyLength));
            // Act
            var key = generator.Next(7);

            // Assert
            var validator = new LuhnModNValidator(new LuhnModNConfig(charSet, keyLength));
            Assert.True(validator.Validate(key));
        }
    }
}
