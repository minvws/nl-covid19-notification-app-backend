// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings
{
    public class StandardEksConfig : AppSettingsReader, IEksConfig
    {
        public StandardEksConfig(IConfiguration config, string prefix = "ExposureKeySets") : base(config, prefix) { }
        public int TekCountMin => GetConfigValue("TekCount:Min", 10);
        public int TekCountMax => GetConfigValue("TekCount:Max", 21); //Low so the file split is tested
        public int LifetimeDays => GetConfigValue("LifetimeDays", 14);
    }
}