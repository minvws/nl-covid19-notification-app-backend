// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class ManifestConfig : AppSettingsReader, IManifestConfig
    {
        public ManifestConfig(IConfiguration config, string? prefix = "Manifest") : base(config, prefix)
        {
        }

        public int KeepAliveCount => GetConfigValue(nameof(KeepAliveCount), 1);
    }
}