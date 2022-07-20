// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class LocalMachineStoreCertificateProvider : ICertificateProvider, IAuthenticationCertificateProvider
    {
        private readonly ILogger _logger;

        public LocalMachineStoreCertificateProvider(ILogger<LocalMachineStoreCertificateProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public X509Certificate2 GetCertificate(string thumbprint, bool rootTrusted)
        {
            X509Certificate2 result;

            _logger.LogInformation("Finding certificate - Thumbprint: {Thumbprint}, RootTrusted: {RootTrusted}.",
                thumbprint, rootTrusted);

            try
            {
                using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);

                result = store.Certificates
                    .Find(X509FindType.FindByThumbprint, thumbprint, rootTrusted)
                    .OfType<X509Certificate2>()
                    .FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading certificate store.");
                throw;
            }

            if (result == null)
            {
                _logger.LogCritical("Certificate not found: {Thumbprint}.", thumbprint);
                throw new InvalidOperationException("Certificate not found.");
            }

            if (!result.HasPrivateKey)
            {
                _logger.LogCritical("Certificate has no private key: {Thumbprint}.", thumbprint);
                throw new InvalidOperationException("Private key not found on certificate.");
            }

            return result;
        }
    }
}
