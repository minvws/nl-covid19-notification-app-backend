// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text.Json.Serialization;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig
{
    public class AppConfigArgs
    {
        public int Version { get; set; }

        public DateTime Release { get; set; }
        
        /// <summary>
        /// The time between updates - GETting the manifest, in minutes.
        /// </summary>
        public int ManifestFrequency { get; set; }

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

        /// <summary>
        /// TODO
        /// </summary>
        public int AndroidMinimumKillVersion { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public string iOSMinimumKillVersion { get; set; }
    }
}
