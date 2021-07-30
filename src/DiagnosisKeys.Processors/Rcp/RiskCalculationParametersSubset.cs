// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp
{
    public class RiskCalculationParametersSubset
    {

        [JsonPropertyName("daysSinceOnsetToInfectiousness")]
        public InfectiousnessByDsosPair[] InfectiousnessByDsos { get; set; }
        public InfectiousnessByDsosPair[] InfectiousnessByTest { get; set; }
    }
}
