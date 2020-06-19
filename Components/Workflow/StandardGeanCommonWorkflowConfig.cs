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

        public StandardGeanCommonWorkflowConfig(IConfiguration config) : base(config, "Validation:KeyCount:")
        {
        }
        public int TemporaryExposureKeyCountMin => GetValueInt32("Min", _Defaults.TemporaryExposureKeyCountMin);
        public int TemporaryExposureKeyCountMax => GetValueInt32("Max", _Defaults.TemporaryExposureKeyCountMax);
        public int GracePeriod => GetValueInt32("GracePeriod", _Defaults.TemporaryExposureKeyCountMax);
    }
}