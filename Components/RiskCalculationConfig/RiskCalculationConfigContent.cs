// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    
    /// <summary>
    /// Part of DB Entity
    /// </summary>
    public class RiskCalculationConfigContent
    {
        //Range 0-8
        [JsonProperty("minimumRiskScore")]
        public int MinimumRiskScore { get; set; }

        //Might not be int..
        [JsonProperty("attenuationScores​")]
        public int[] AttenuationScores​ { get; set; }

        //Might not be int..
        [JsonProperty("daysSinceLastExposureScores​")]
        public int[] DaysSinceLastExposureScores​ { get; set; }

        //Might not be int..
        [JsonProperty("durationScores")]
        public int[] DurationScores { get; set; }

        //Might not be int..
        [JsonProperty("transmissionRiskScores​")]
        public int[] TransmissionRiskScores​ { get; set; }

        //Might not be int..
        //length 2
        [JsonProperty("durationAtAttenuationThresholds​")]
        public int[] DurationAtAttenuationThresholds​ { get; set; }
    }
}