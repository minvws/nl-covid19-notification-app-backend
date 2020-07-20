// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TemporaryExposureKeyValidatorConfig : AppSettingsReader, ITemporaryExposureKeyValidatorConfig
    {
        public TemporaryExposureKeyValidatorConfig(IConfiguration config, string prefix = DefaultPrefix) : base(config, prefix)
        {
        }

        private const string DefaultPrefix = "Validation:TemporaryExposureKey:";

        public int RollingPeriodMin => GetConfigValue("RollingPeriod:Min", 1);
        public int RollingPeriodMax => GetConfigValue("RollingPeriod:Max", 1);
        public int DailyKeyByteCount => GetConfigValue(nameof(DailyKeyByteCount), 16);
    }
}