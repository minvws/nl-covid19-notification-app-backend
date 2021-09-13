// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Iks
{
    public class IksCleaningConfig : AppSettingsReader, IIksCleaningConfig
    {
        private const int DefaultLifetimeDays = 14;

        public IksCleaningConfig(IConfiguration config, string prefix = "Iks") : base(config, prefix)
        {
        }

        public int LifetimeDays => GetConfigValue(nameof(LifetimeDays), DefaultLifetimeDays);
    }
}
