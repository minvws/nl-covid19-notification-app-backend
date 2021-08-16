// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace Endpoint.Tests.ContentModels
{
    public class RiskCalculationParameters
    {
        public DaysSinceOnsetToInfectiousness[] daysSinceOnsetToInfectiousness { get; set; }
        public int infectiousnessWhenDaysSinceOnsetMissing { get; set; }
        public int minimumWindowScore { get; set; }
        public int daysSinceExposureThreshold { get; set; }
        public int[] attenuationBucketThresholds { get; set; }
        public double[] attenuationBucketWeights { get; set; }
        public double[] infectiousnessWeights { get; set; }
        public double[] reportTypeWeights { get; set; }
        public int reportTypeWhenMissing { get; set; }
    }

    public class DaysSinceOnsetToInfectiousness
    {
        public int daysSinceOnsetOfSymptoms { get; set; }
        public int infectiousness { get; set; }
    }
}
