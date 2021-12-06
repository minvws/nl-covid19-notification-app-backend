// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using Xunit;

namespace MobileAppApi.Tests
{
    public class TemporaryExposureKeyValidatorTests
    {
        [Theory]
        [InlineData(1)] // RollingPeriod is min value       
        [InlineData(144)] // RollingPeriod is min value       
        public void Teks_Edgecases_Are_Valid(int rollingPeriod)
        {
            //Arrange
            var rollingStartNumber = DateTime.UtcNow.Date.ToRollingStartNumber();
            var config = new DefaultTekValidatorConfig();
            var dateTimeProvider = new StandardUtcDateTimeProvider();
            var tekValidator = new TemporaryExposureKeyValidator(config, dateTimeProvider);

            var postTeksItemArgs = new PostTeksItemArgs()
            {
                RollingStartNumber = rollingStartNumber,
                RollingPeriod = rollingPeriod,
                KeyData = Convert.ToBase64String(new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })
            };

            // Act
            var result = tekValidator.Valid(postTeksItemArgs);

            // Assert
            Assert.False(result.Any());
        }
    }
}
