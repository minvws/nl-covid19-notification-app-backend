// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow
{
    [TestClass()]
    public class NewTeksRiskCalculationV1Tests
    {
        private readonly DateTime _DateOfSystemsOnset = new DateTime(2020, 1, 10);


        [DataRow(5, TransmissionRiskLevel.None)]
        [DataRow(6, TransmissionRiskLevel.None)]
        
        [DataRow(7, TransmissionRiskLevel.None)]

        [DataRow(8, TransmissionRiskLevel.Medium)]

        [DataRow(9, TransmissionRiskLevel.High)]
        [DataRow(10, TransmissionRiskLevel.High)]
        [DataRow(11, TransmissionRiskLevel.High)]
        [DataRow(12, TransmissionRiskLevel.High)]

        [DataRow(13, TransmissionRiskLevel.Medium)]
        [DataRow(14, TransmissionRiskLevel.Medium)]

        [DataRow(15, TransmissionRiskLevel.Low)]
        [DataRow(16, TransmissionRiskLevel.Low)]
        [DataRow(17, TransmissionRiskLevel.Low)]
        [DataRow(18, TransmissionRiskLevel.Low)]
        [DataRow(19, TransmissionRiskLevel.Low)]
        [DataRow(20, TransmissionRiskLevel.Low)]
        [DataRow(21, TransmissionRiskLevel.Low)]

        [DataRow(22, TransmissionRiskLevel.None)]
        [DataRow(23, TransmissionRiskLevel.None)]

        [TestMethod]
        public void RiskLevel(int day, TransmissionRiskLevel expected)
        {
            var tek = new Tek { RollingStartNumber = new DateTime(2020, 1, day, 0,0,0, DateTimeKind.Utc).ToRollingStartNumber() };
            var calculation = new TransmissionRiskLevelCalculationV1();
            var result = calculation.Calculate(tek.RollingStartNumber, _DateOfSystemsOnset);
            Assert.AreEqual(expected, result);
        }
    }
}