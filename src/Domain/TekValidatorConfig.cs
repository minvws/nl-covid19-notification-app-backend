// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class TekValidatorConfig : AppSettingsReader, ITekValidatorConfig
    {

        private static readonly DefaultTekValidatorConfig defaults = new DefaultTekValidatorConfig();

        public TekValidatorConfig(IConfiguration config, string prefix = DefaultPrefix) : base(config, prefix)
        {
        }

        private const string DefaultPrefix = "Workflow:PostKeys:TemporaryExposureKeys";
        public int RollingStartNumberMin => GetConfigValue("RollingStartNumber:Min", defaults.RollingStartNumberMin);
        public int PublishingDelayInMinutes => GetConfigValue("PublishingDelayMinutes", defaults.PublishingDelayInMinutes);
        public int AuthorisationWindowMinutes => GetConfigValue("AuthorisationWindowMinutes", defaults.AuthorisationWindowMinutes);
        public int MaxAgeDays => GetConfigValue(nameof(MaxAgeDays), defaults.MaxAgeDays);
    }
}
