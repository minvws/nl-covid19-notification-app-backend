// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Tests
{
    public class NewTeksRiskCalculationV1Tests
    {
        [Theory]
        [InlineData(-5, TransmissionRiskLevel.None)]
        [InlineData(-4, TransmissionRiskLevel.None)]
        [InlineData(-3, TransmissionRiskLevel.None)]

        [InlineData(-2, TransmissionRiskLevel.Medium)] //-2

        [InlineData(-1, TransmissionRiskLevel.High)] //0
        [InlineData(0, TransmissionRiskLevel.High)] //0
        [InlineData(1, TransmissionRiskLevel.High)] //0
        [InlineData(2, TransmissionRiskLevel.High)] //0

        [InlineData(3, TransmissionRiskLevel.Medium)] //3
        [InlineData(4, TransmissionRiskLevel.Medium)] //3

        [InlineData(5, TransmissionRiskLevel.Low)] //8
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

        //TRL M
        [InlineData(-2, -2)] //-2

        //TRL High
        [InlineData(-1, 0)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        
        //TRL M
        [InlineData(3, 3)] //3 
        [InlineData(4, 3)] //3

        //TRL L
        [InlineData(5, 8)]
        [InlineData(6, 8)]
        [InlineData(7, 8)]
        [InlineData(8, 8)] //Mid point
        [InlineData(9, 8)]
        [InlineData(10, 8)]
        [InlineData(11, 8)]
        public void InputForEfgsDsos(int day, int expected)
        {
            var calculation = new DosViaTrlDayRangeMidPointCalculation();
            var result = calculation.Calculate(day);
            Assert.Equal(expected, result);
        }
    }
}