// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    public class ResourceBundleContent
    {
        //TODO Version of what? Remove
        public string Version { get; set; }

        
        //TODO go to AppConfig?
        public int IsolationPeriodDays { get; set; }

        public Dictionary<string,Dictionary<string,string>> Text { get; set; }
    }
}