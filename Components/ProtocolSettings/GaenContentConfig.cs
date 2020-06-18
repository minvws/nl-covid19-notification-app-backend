// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings
{
    public class GaenContentConfig : AppSettingsReader, IGaenContentConfig
    {
        public GaenContentConfig(IConfiguration config) : base(config) { }

        public int ExposureKeySetCapacity => GetValueInt32("Gaen:ExposureKeySet:CapacityKeyCount", 21);
        public double ManifestLifetimeHours { get; set; }
        public int ExposureKeySetLifetimeDays => GetValueInt32("Gaen:ExposureKeySet:LifetimeDays", 21);
        public int SecretLifetimeDays => GetValueInt32("Gaen:Workflow:LifetimeDays", 12);
    }
}