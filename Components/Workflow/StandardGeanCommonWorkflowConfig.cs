// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class StandardGeanCommonWorkflowConfig : AppSettingsReader, IGeanTekListValidationConfig
    {
        private readonly IGeanTekListValidationConfig _Defaults = new DefaultGeanTekListValidationConfig();

        public StandardGeanCommonWorkflowConfig(IConfiguration config, string prefix = "Workflow:PostKeys:TemporaryExposureKeys") : base(config, prefix)
        {
        }
        
        public int TemporaryExposureKeyCountMin => GetConfigValue("Count:Min", _Defaults.TemporaryExposureKeyCountMin);
        public int TemporaryExposureKeyCountMax => GetConfigValue("Count:Max", _Defaults.TemporaryExposureKeyCountMax);
        public int GracePeriodMinutes => GetConfigValue(nameof(GracePeriodMinutes), _Defaults.TemporaryExposureKeyCountMax);
    }
}