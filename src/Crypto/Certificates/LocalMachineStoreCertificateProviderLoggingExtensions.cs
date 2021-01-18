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
        private string Name = "LocalMachineStoreCertificateProvider";
        private const int Base = LoggingCodex.CertLmProvider;

        private const int Finding = Base + 1;
        private const int CertNotFound = Base + 2;
        private const int NoPrivateKey = Base + 3;
        private const int CertReadError = Base + 4;

        private readonly ILogger _Logger;

        public LocalMachineStoreCertificateProviderLoggingExtensions(ILogger<LocalMachineStoreCertificateProviderLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteFindingCert(string thumbprint, bool rootTrusted)
        {
            _Logger.LogInformation("[{name}/{id}] Finding certificate - Thumbprint:{Thumbprint}, RootTrusted:{RootTrusted}.",
                Name, Finding,
                thumbprint, rootTrusted);
        }

        public void WriteCertNotFound(string thumbprint)
        {
            _Logger.LogCritical("[{name}/{id}] Certificate not found: {Thumbprint}.",
                Name, CertNotFound,
                thumbprint);
        }

        public void WriteNoPrivateKey(string thumbprint)
        {
            _Logger.LogCritical("[{name}/{id}] Certificate has no private key: {Thumbprint}.",
                Name, NoPrivateKey,
                thumbprint);
        }

        public void WriteCertReadError(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _Logger.LogError(exception, "[{name}/{id}] Error reading certificate store.",
                Name, CertReadError);
        }
    }
}