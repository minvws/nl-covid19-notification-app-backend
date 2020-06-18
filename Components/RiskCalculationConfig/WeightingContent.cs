// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class WeightingContent
    {
        public int Weight { get; set; }
        public int[] LevelValues { get; set; }//:[1, 2, 3, 4, 5, 6, 7, 8],
    }
}