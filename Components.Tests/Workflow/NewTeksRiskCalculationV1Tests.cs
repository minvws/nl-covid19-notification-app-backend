// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow
{
    public class NewTeksRiskCalculationV1Tests
    {
        [Theory]
        [InlineData(-5, TransmissionRiskLevel.None)]
        [InlineData(-4, TransmissionRiskLevel.None)]
        [InlineData(-3, TransmissionRiskLevel.None)]

        [InlineData(-2, TransmissionRiskLevel.Medium)]

        [InlineData(-1, TransmissionRiskLevel.High)]
        [InlineData(0, TransmissionRiskLevel.High)]
        [InlineData(1, TransmissionRiskLevel.High)]
        [InlineData(2, TransmissionRiskLevel.High)]

        [InlineData(3, TransmissionRiskLevel.Medium)]
        [InlineData(4, TransmissionRiskLevel.Medium)]

        [InlineData(5, TransmissionRiskLevel.Low)]
        [InlineData(6, TransmissionRiskLevel.Low)]
        [InlineData(7, TransmissionRiskLevel.Low)]
        [InlineData(8, TransmissionRiskLevel.Low)]
        [InlineData(9, TransmissionRiskLevel.Low)]
        [InlineData(10, TransmissionRiskLevel.Low)]
        [InlineData(11, TransmissionRiskLevel.Low)]

        [InlineData(12, TransmissionRiskLevel.None)]
        [InlineData(13, TransmissionRiskLevel.None)]

        public void RiskLevel(int day, TransmissionRiskLevel expected)
        {
            var calculation = new TransmissionRiskLevelCalculationMk2();
            var result = calculation.Calculate(day);
            Assert.Equal(expected, result);
        }
    }
}