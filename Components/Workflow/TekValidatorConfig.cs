// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TekValidatorConfig : AppSettingsReader, ITekValidatorConfig
    {

        private static readonly DefaultTekValidatorConfig _Defaults = new DefaultTekValidatorConfig();

        public TekValidatorConfig(IConfiguration config, string prefix = DefaultPrefix) : base(config, prefix)
        {
        }

        private const string DefaultPrefix = "Workflow:PostKeys:TemporaryExposureKeys";

        public int RollingPeriodMin => GetConfigValue("RollingPeriod:Min", _Defaults.RollingPeriodMin);
        public int RollingPeriodMax => GetConfigValue("RollingPeriod:Max", _Defaults.RollingPeriodMax);

        public int RollingStartNumberMin => GetConfigValue("RollingStartNumber:Min", _Defaults.RollingStartNumberMin);
        public int MaxAgeDays => GetConfigValue(nameof(MaxAgeDays), _Defaults.MaxAgeDays);
        public int KeyDataLength => GetConfigValue(nameof(KeyDataLength), _Defaults.KeyDataLength);
    }
}