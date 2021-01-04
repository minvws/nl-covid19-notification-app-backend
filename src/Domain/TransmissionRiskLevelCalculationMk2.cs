// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class TransmissionRiskLevelCalculationMk2 : ITransmissionRiskLevelCalculationMk2
    {
        private static readonly Range<int> _SignificantDayRange = new Range<int>(-2, 11);

        public Range<int> SignificantDayRange => _SignificantDayRange;

        public TransmissionRiskLevel Calculate(int daysSinceOnsetSymptoms)
        {
            //Keys before date of onset
            if (daysSinceOnsetSymptoms <= -3) return TransmissionRiskLevel.None;
            if (daysSinceOnsetSymptoms <= _SignificantDayRange.Lo) return TransmissionRiskLevel.Medium;
            if (daysSinceOnsetSymptoms <= 2) return TransmissionRiskLevel.High;
            if (daysSinceOnsetSymptoms <= 4) return TransmissionRiskLevel.Medium;
            if (daysSinceOnsetSymptoms <= _SignificantDayRange.Hi) return TransmissionRiskLevel.Low;
            return TransmissionRiskLevel.None;
            //Keys after date of onset
        }
    }
}