// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class RiskCalculationConfigArgs
    {
        public DateTime Release { get; set; }

        //Range 0-8
        public int MinimumRiskScore { get; set; }

        //Might not be int..
        public int[] AttenuationScores​ { get; set; }

        //Might not be int..
        public int[] DaysSinceLastExposureScores​ { get; set; }

        //Might not be int..
        public int[] DurationScores { get; set; }

        //Might not be int..
        public int[] TransmissionRiskScores​ { get; set; }

        //Might not be int..
        //length 2
        public int[] DurationAtAttenuationThresholds​ { get; set; }
    }
}