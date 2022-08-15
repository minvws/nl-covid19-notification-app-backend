// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class FileSystemCertificateProvider : IAuthenticationCertificateProvider
    {
        private readonly ICertificateConfig _config;
        private readonly ILogger _logger;

        public FileSystemCertificateProvider(
            ICertificateConfig config,
            ILogger<FileSystemCertificateProvider> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public X509Certificate2 GetCertificate()
        {
            var paths = new[] { _config.CertificatePath, _config.CertificateFileName };
            var fullPath = Path.Combine(paths);

            using var f = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            if (f == null)
            {
                _logger.LogError("Failed to get filestream");
                throw new InvalidOperationException("Could not find file.");
            }

            _logger.LogInformation("File found");
            var bytes = new byte[f.Length];
            f.Read(bytes, 0, bytes.Length);

            return new X509Certificate2(bytes);
        }
    }
}
