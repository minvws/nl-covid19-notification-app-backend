// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings
{
    public class CdnContentConfig : AppSettingsReader, ICdnContentConfig
    {
        public CdnContentConfig(IConfiguration config) : base(config, "AppSettings")
        {
            
        }

        public double ExposureKeySetLifetimeDays => GetValueDouble("ExposureKeySetLifetimeDays", 20);
        public double ContentLifetimeDays => GetValueDouble("ContentLifetimeDays", 10);
    }
}
