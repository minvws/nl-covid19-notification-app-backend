// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow
{
    public class NewTeksRiskCalculationV1Tests
    {
        private readonly DateTime _DateOfSystemsOnset = new DateTime(2020, 1, 10);

        [Theory]
        [InlineData(5, TransmissionRiskLevel.None)]
        [InlineData(6, TransmissionRiskLevel.None)]
        
        [InlineData(7, TransmissionRiskLevel.None)]

        [InlineData(8, TransmissionRiskLevel.Medium)]

        [InlineData(9, TransmissionRiskLevel.High)]
        [InlineData(10, TransmissionRiskLevel.High)]
        [InlineData(11, TransmissionRiskLevel.High)]
        [InlineData(12, TransmissionRiskLevel.High)]

        [InlineData(13, TransmissionRiskLevel.Medium)]
        [InlineData(14, TransmissionRiskLevel.Medium)]

        [InlineData(15, TransmissionRiskLevel.Low)]
        [InlineData(16, TransmissionRiskLevel.Low)]
        [InlineData(17, TransmissionRiskLevel.Low)]
        [InlineData(18, TransmissionRiskLevel.Low)]
        [InlineData(19, TransmissionRiskLevel.Low)]
        [InlineData(20, TransmissionRiskLevel.Low)]
        [InlineData(21, TransmissionRiskLevel.Low)]

        [InlineData(22, TransmissionRiskLevel.None)]
        [InlineData(23, TransmissionRiskLevel.None)]

        public void RiskLevel(int day, TransmissionRiskLevel expected)
        {
            var tek = new Tek { RollingStartNumber = new DateTime(2020, 1, day, 0,0,0, DateTimeKind.Utc).ToRollingStartNumber() };
            var calculation = new TransmissionRiskLevelCalculationV1();
            var result = calculation.Calculate(tek.RollingStartNumber, _DateOfSystemsOnset);
            Assert.Equal(expected, result);
        }
    }
}