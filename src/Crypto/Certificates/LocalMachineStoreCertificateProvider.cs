// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class LocalMachineStoreCertificateProvider : ICertificateProvider, IAuthenticationCertificateProvider
    {
        private readonly LocalMachineStoreCertificateProviderLoggingExtensions _logger;

        public LocalMachineStoreCertificateProvider(LocalMachineStoreCertificateProviderLoggingExtensions logger)
        {
            _logger = logger;
        }

        public X509Certificate2 GetCertificate(string thumbprint, bool rootTrusted)
        {
            X509Certificate2 result;

            _logger.WriteFindingCert(thumbprint, rootTrusted);

            try
            {
                using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                result = store.Certificates
                    .Find(X509FindType.FindByThumbprint, thumbprint, rootTrusted)
                    .OfType<X509Certificate2>()
                    .FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.WriteCertReadError(e);
                throw;
            }

            if (result == null)
            {
                _logger.WriteCertNotFound(thumbprint);
                throw new InvalidOperationException("Certificate not found.");
            }

            if (!result.HasPrivateKey)
            {
                _logger.WriteNoPrivateKey(thumbprint);
                throw new InvalidOperationException("Private key not found on certificate.");
            }

            return result;
        }
    }
}
