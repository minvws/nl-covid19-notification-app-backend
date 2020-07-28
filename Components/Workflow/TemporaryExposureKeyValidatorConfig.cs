// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TemporaryExposureKeyValidatorConfig : AppSettingsReader, ITemporaryExposureKeyValidatorConfig
    {

        private static readonly DefaultGaenTekValidatorConfig Defaults = new DefaultGaenTekValidatorConfig();

        public TemporaryExposureKeyValidatorConfig(IConfiguration config, string prefix = DefaultPrefix) : base(config, prefix)
        {
        }

        private const string DefaultPrefix = "Workflow:PostKeys:TemporaryExposureKeys";

        public int RollingPeriodMin => GetConfigValue("RollingPeriod:Min", Defaults.RollingPeriodMin);
        public int RollingPeriodMax => GetConfigValue("RollingPeriod:Max", Defaults.RollingPeriodMax);
        public int DailyKeyByteCount => GetConfigValue(nameof(DailyKeyByteCount), Defaults.DailyKeyByteCount);
    }
}