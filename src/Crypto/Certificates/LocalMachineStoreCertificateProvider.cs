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
        private readonly IThumbprintConfig _ThumbprintConfig;
        private readonly LocalMachineStoreCertificateProviderLoggingExtensions _Logger;

        public LocalMachineStoreCertificateProvider(IThumbprintConfig thumbprintConfig, LocalMachineStoreCertificateProviderLoggingExtensions logger)
        {
            _ThumbprintConfig = thumbprintConfig ?? throw new ArgumentNullException(nameof(thumbprintConfig));
            _Logger = logger;
        }

        public X509Certificate2 GetCertificate()
        {
            using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            _Logger.WriteFindingCert(_ThumbprintConfig.Thumbprint, _ThumbprintConfig.RootTrusted);

            var result = ReadCertFromStore(store);
            if (result == null)
            {
                _Logger.WriteCertNotFound(_ThumbprintConfig.Thumbprint);
                throw new InvalidOperationException("Certificate not found.");
            }

            if (!result.HasPrivateKey)
            {
                _Logger.WriteNoPrivateKey(_ThumbprintConfig.Thumbprint);
                throw new InvalidOperationException("Private key not found.");
            }

            return result;
        }
        private X509Certificate2 ReadCertFromStore(X509Store x509Store)
        {
            try
            {
                return x509Store.Certificates
                    .Find(X509FindType.FindByThumbprint, _ThumbprintConfig.Thumbprint, _ThumbprintConfig.RootTrusted)
                    .OfType<X509Certificate2>()
                    .FirstOrDefault();
            }
            catch (Exception e)
            {
                _Logger.WriteCertReadError(e);
                throw;
            }
        }

    }
}