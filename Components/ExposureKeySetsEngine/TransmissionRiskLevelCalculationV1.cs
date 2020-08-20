// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class TransmissionRiskLevelCalculationV1 : ITransmissionRiskLevelCalculation
    {
        public TransmissionRiskLevel Calculate(int tekRollingPeriodNumber, DateTime dateOfSymptomsOnset)
        {
            if (dateOfSymptomsOnset.Date != dateOfSymptomsOnset)
                throw new ArgumentException("Not a date.", nameof( dateOfSymptomsOnset));

            var daysSinceSymptomOnset =
                Convert.ToInt32(Math.Floor((tekRollingPeriodNumber.FromRollingStartNumber().Date - dateOfSymptomsOnset).TotalDays));

            //Keys before date of onset
            if (daysSinceSymptomOnset <= -3) return TransmissionRiskLevel.None;
            if (daysSinceSymptomOnset <= -2) return TransmissionRiskLevel.Medium;
            if (daysSinceSymptomOnset <= 2) return TransmissionRiskLevel.High;
            if (daysSinceSymptomOnset <= 4) return TransmissionRiskLevel.Medium;
            if (daysSinceSymptomOnset <= 11) return TransmissionRiskLevel.Low;
            return TransmissionRiskLevel.None;
            //Keys after date of onset
        }
    }
}