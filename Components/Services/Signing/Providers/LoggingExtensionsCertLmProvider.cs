// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.LocalMachineStoreCertificateProvider
{
    public static class LoggingExtensionsLocalMachineStoreCertificateProvider
    {

        private static string Name = "LocalMachineStoreCertificateProvider";
        private const int Base = LoggingCodex.CertLmProvider;

        private const int Finding = Base;
        private const int CertNotFound = Base + 1;
        private const int NoPrivateKey = Base + 2;
        private const int CertReadError = Base + 3;

        public static void WriteFindingCert(this ILogger logger, string thumbprint, bool rootTrusted)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Finding certificate - Thumbprint:{Thumbprint}, RootTrusted:{RootTrusted}.",
                Name, Finding,
                thumbprint, rootTrusted);
        }

        public static void WriteCertNotFound(this ILogger logger, string thumbprint)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogCritical("[{name}/{id}] Certificate not found: {Thumbprint}.",
                Name, CertNotFound,
                thumbprint);
        }

        public static void WriteNoPrivateKey(this ILogger logger, string thumbprint)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogCritical("[{name}/{id}] Certificate has no private key: {Thumbprint}.",
                Name, NoPrivateKey,
                thumbprint);
        }

        public static void WriteCertReadError(this ILogger logger, Exception exception)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError(exception, "[{name}/{id}] Error reading certificate store.",
                Name, CertReadError);
        }
    }
}