// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class CertificateConfig : AppSettingsReader, ICertificateConfig
    {
        public CertificateConfig(IConfiguration config, string prefix = "Certificates") : base(config, prefix)
        {
        }

        public string CertificatePath => GetConfigValue(nameof(CertificatePath), string.Empty);
        public string CertificateFileName => GetConfigValue(nameof(CertificateFileName), string.Empty);

    }
}