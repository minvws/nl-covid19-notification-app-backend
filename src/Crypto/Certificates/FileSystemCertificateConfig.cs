// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class FileSystemCertificateConfig : AppSettingsReader, IFileSystemCertificateConfig
    {
        public FileSystemCertificateConfig(IConfiguration config, string prefix = null) : base(config, prefix)
        {
        }

        public string CertificatePath => GetConfigValue(nameof(CertificatePath), string.Empty);
        public string CertificateFileName => GetConfigValue(nameof(CertificateFileName), string.Empty);

    }
}
