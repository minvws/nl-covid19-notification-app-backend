// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestContent
    {
        [JsonProperty("exposureKeySets")]
        public string[] ExposureKeySets { get; set; }
        
        [JsonProperty("resourceBundle")]
        public string ResourceBundle { get; set; }
        
        [JsonProperty("riskCalculationParameters")]
        public string RiskCalculationParameters { get; set; }

        [JsonProperty("appConfig")]
        public string AppConfig { get; set; }
    }
}