// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class RiskCalculationConfigArgs
    {
        public int MinimumRiskScore { get; set; }
        public WeightingArgs Attenuation { get; set; }
        public WeightingArgs DaysSinceLastExposure { get; set; }
        public WeightingArgs DurationLevelValues { get; set; }
        public WeightingArgs TransmissionRisk { get; set; }
        public DateTime Release { get; set; }
    }
}