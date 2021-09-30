// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class EmbeddedResourceCertificateProvider : ICertificateProvider
    {
        private readonly ICertificateChainConfig _config;
        private readonly EmbeddedCertProviderLoggingExtensions _logger;

        public EmbeddedResourceCertificateProvider(ICertificateChainConfig config, EmbeddedCertProviderLoggingExtensions logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public X509Certificate2 GetCertificate(string thumbprint, bool rootTrusted)
        {
            //This matches the assembly base namespace and the folder name of the resource files.
            using var s = ResourcesHook.GetManifestResourceStream(_config.Path);
            if (s == null)
            {
                _logger.WriteResourceFail();
                throw new InvalidOperationException("Could not find resource.");
            }

            _logger.WriteResourceFound();
            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);
            return new X509Certificate2(bytes, _config.Password, X509KeyStorageFlags.Exportable);
        }
    }
}
