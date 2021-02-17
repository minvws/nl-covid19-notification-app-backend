// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class LocalMachineStoreCertificateProviderConfig : AppSettingsReader, IThumbprintConfig
    {
        public LocalMachineStoreCertificateProviderConfig(IConfiguration config, string? prefix = null) : base(config, prefix) { }

        public string Thumbprint => GetConfigValue<string>(nameof(Thumbprint));
        public bool RootTrusted => GetConfigValue<bool>(nameof(RootTrusted));
    }
}