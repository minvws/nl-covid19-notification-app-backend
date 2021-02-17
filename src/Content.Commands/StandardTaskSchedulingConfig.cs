// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class StandardTaskSchedulingConfig : AppSettingsReader, ITaskSchedulingConfig
    {

        private static readonly ProductionDefaultValuesTaskSchedulingConfig _ProductionDefaultValues = new ProductionDefaultValuesTaskSchedulingConfig();

        public StandardTaskSchedulingConfig(IConfiguration config, string prefix = "TaskScheduling") : base(config, prefix)
        {
        }

        /// <summary>
        /// Replace missing or unparsable setting value with the production default.
        /// TODO consider if this hides a potential issue - alternative is to throw a MissingCOnfigValueEx.
        /// </summary>
        public double DailyCleanupHoursAfterMidnight
        {
            get
            {
                try
                {
                    var raw = GetConfigValue("DailyCleanupStartTime", "Not a time.");
                    var result = DateTime.ParseExact(raw, "hh:mm:ss", CultureInfo.InvariantCulture);
                    return result.TimeOfDay.TotalHours;
                }
                catch (FormatException)
                {
                    return _ProductionDefaultValues.DailyCleanupHoursAfterMidnight;
                }
            }
        }
    }
}