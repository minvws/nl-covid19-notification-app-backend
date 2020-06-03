// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    
    /// <summary>
    /// Part of DB Entity
    /// </summary>
    public class RiskCalculationConfigContent
    {
        public int MinimumRiskScore { get; set; }
        public WeightingContent Attenuation { get; set; }
        public WeightingContent DaysSinceLastExposure { get; set; }
        public WeightingContent DurationLevelValues { get; set; }
        public WeightingContent TransmissionRisk { get; set; }
    }
}