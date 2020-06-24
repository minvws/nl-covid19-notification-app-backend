// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    public class ResourceBundleContent
    {
        [JsonProperty("version")]
        //TODO Version of what? Remove
        public string Version { get; set; }

        
        //TODO go to AppConfig?
        [JsonProperty("isolationPeriodDays")]
        public int IsolationPeriodDays { get; set; }

        [JsonProperty("text")]
        public Dictionary<string,Dictionary<string,string>> Text { get; set; }
    }
}