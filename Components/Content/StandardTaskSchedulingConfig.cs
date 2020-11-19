// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class StandardTaskSchedulingConfig : AppSettingsReader, ITaskSchedulingConfig
    {
        public StandardTaskSchedulingConfig(IConfiguration config, string? prefix = "TaskScheduling") : base(config, prefix)
        {
        }

        public double DailyCleanupHoursAfterMidnight
        {
            get
            {
                var raw = GetConfigValue("DailyCleanupStartTime", "05:00:00");
                var result = DateTime.ParseExact(raw, "hh:mm:ss", CultureInfo.InvariantCulture);
                return result.TimeOfDay.TotalHours;
            }
        }
    }
}