// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text.Json.Serialization;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig
{
    public class AppConfigArgs
    {
        public DateTime Release { get; set; }

        /// <summary>
        /// The minimum version of the app.The app has a hardcoded version number that is increased by 1  on each app release. Whenever the app downloads the manifest, it must compare its hardcoded version  number with that of the manifest. If the hardcoded version number is less than the manifest value, the user will be asked to upgrade the app from the app store.See https://github.com/minvws/nl-covid19-notification-app-coordination/blob/master/architecture/Solution%20Architecture.md#lifecycle-management.
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// The time between updates - GETting the manifest, in minutes.
        /// </summary>
        public int UpdatePeriodMinutes { get; set; }

        /// <summary>
        /// This defines the probability of sending decoys. This is configurable so we can tune the probability to server load if necessary.
        /// TODO is this a percentage?
        /// </summary>
        public int DecoyProbability { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public int TemporaryExposureKeyRetentionDays { get; set; }
        /// <summary>
        /// TODO
        /// </summary>
        public int ObservedTemporaryExposureKeyRetentionDays { get; set; }
    }
}
