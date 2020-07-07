// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestContent
    {
        public string[] ExposureKeySets { get; set; }
        
        public string ResourceBundle { get; set; }
        
        public string RiskCalculationParameters { get; set; }

        public string AppConfig { get; set; }
    }
}