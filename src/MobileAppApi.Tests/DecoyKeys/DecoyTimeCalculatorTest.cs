// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Xunit;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.DecoyKeys
{
    public class DecoyTimeCalculatorTest
    {
        [Fact]
        public void GetDelay_NoNegativeValues()
        {
            //Arrange
            var loggerFactory = new LoggerFactory();
            var algorithmMock = new Mock<IWelfordsAlgorithm>();
            algorithmMock.Setup(x => x.GetNormalSample())
                .Returns(-1000);

            var sut = new DecoyTimeCalculator(
                new DecoyKeysLoggingExtensions(loggerFactory.CreateLogger<DecoyKeysLoggingExtensions>()),
                algorithmMock.Object);
            
            //Act
            var result = sut.GetDelay();

            //Assert
            Assert.True(result.Milliseconds == 0);
        }
    }
}
