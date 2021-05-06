// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components.Tests
{
    public class PublishTekArgsValidatorTests
    {
        [Fact]
        public void GenerateTest()
        {
            var c = new LuhnModNConfig();
            var g = new LuhnModNGenerator(c);
            var result = g.Next(6);
            var v = new LuhnModNValidator(c);
            Assert.True(v.Validate(result));
        }


        [InlineData("L8T6LJQ")]
        [InlineData("XBB5XFJ")]
        [InlineData("8JTRXXG")]
        [Theory]
        public void Input_With_Correct7FigureCode_ReturnsValid_For_LuhnModN_Check(string pubTek)
        {
            // Arrange
            var validator = new PublishTekArgsValidator(new LuhnModNValidator(new LuhnModNConfig()), new  StandardUtcDateTimeProvider());
            var args = new PublishTekArgs
            {
                GGDKey = pubTek,
                SelectedDate = DateTime.Today,
                Symptomatic = true
            };

            // Act
            var errorMessages = validator.Validate(args);
            
            // Assert
            Assert.True(errorMessages.Length == 0);
        }

        [InlineData("L8T6LJR")]
        [InlineData("XBB5XFT")]
        [InlineData("8JTRXX5")]
        [Theory]
        public void Input_With_InCorrect7FigureCode_ReturnsInValid_For_LuhnModN_Check(string pubTek)
        {
            // Arrange
            var validator = new PublishTekArgsValidator(new LuhnModNValidator(new LuhnModNConfig()), new StandardUtcDateTimeProvider());
            var args = new PublishTekArgs
            {
                GGDKey = pubTek,
                SelectedDate = DateTime.Today,
                Symptomatic = true
            };

            // Act
            var errorMessages = validator.Validate(args);

            // Assert
            Assert.False(errorMessages.Length == 0);
        }
    }
}
