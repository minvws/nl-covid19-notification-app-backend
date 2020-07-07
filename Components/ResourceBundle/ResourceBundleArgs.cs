// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    public class ResourceBundleArgs
    {
        public DateTime Release { get; set; }
        public int IsolationPeriodDays { get; set; }
        // TODO define this properly
        [JsonIgnore]
        public Dictionary<string,Dictionary<string,string>>? Text { get; set; }
    }
}