// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class RiskCalculationConfigResponse
    {
        public int MinimumRiskScore { get; set; }
        public WeightingResponse Attenuation { get; set; }
        public WeightingResponse DaysSinceLastExposure { get; set; }
        public WeightingResponse DurationLevelValues { get; set; }
        public WeightingResponse TransmissionRisk { get; set; }
    }
}