// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class EmbeddedResourceCertificateConfig : AppSettingsReader, IEmbeddedResourceCertificateConfig
    {
        public EmbeddedResourceCertificateConfig(IConfiguration config, string prefix = null) : base(config, prefix)
        {
        }

        public string Path => GetConfigValue<string>(nameof(Path));
        public string Password => GetConfigValue<string>(nameof(Password));
    }
}