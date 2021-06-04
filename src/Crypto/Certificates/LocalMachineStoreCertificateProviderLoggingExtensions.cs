// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class LocalMachineStoreCertificateProviderLoggingExtensions
    {
        private readonly string _name = "LocalMachineStoreCertificateProvider";
        private const int Base = LoggingCodex.CertLmProvider;

        private const int Finding = Base + 1;
        private const int CertNotFound = Base + 2;
        private const int NoPrivateKey = Base + 3;
        private const int CertReadError = Base + 4;

        private readonly ILogger _logger;

        public LocalMachineStoreCertificateProviderLoggingExtensions(ILogger<LocalMachineStoreCertificateProviderLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteFindingCert(string thumbprint, bool rootTrusted)
        {
            _logger.LogInformation("[{name}/{id}] Finding certificate - Thumbprint:{Thumbprint}, RootTrusted:{RootTrusted}.",
                _name, Finding,
                thumbprint, rootTrusted);
        }

        public void WriteCertNotFound(string thumbprint)
        {
            _logger.LogCritical("[{name}/{id}] Certificate not found: {Thumbprint}.",
                _name, CertNotFound,
                thumbprint);
        }

        public void WriteNoPrivateKey(string thumbprint)
        {
            _logger.LogCritical("[{name}/{id}] Certificate has no private key: {Thumbprint}.",
                _name, NoPrivateKey,
                thumbprint);
        }

        public void WriteCertReadError(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _logger.LogError(exception, "[{name}/{id}] Error reading certificate store.",
                _name, CertReadError);
        }
    }
}
