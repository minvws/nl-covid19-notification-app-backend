// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    public class HsmCertificateProvider : ICertificateProvider
    {
        private readonly IThumbprintConfig _ThumbprintConfig;
        private readonly ILogger _Logger;

        public HsmCertificateProvider(IThumbprintConfig thumbprintConfig, ILogger<HsmCertificateProvider> logger)
        {
            _ThumbprintConfig = thumbprintConfig ?? throw new ArgumentNullException(nameof(thumbprintConfig));
            _Logger = logger;
        }

        public X509Certificate2? GetCertificate()
        {
            using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            _Logger.LogInformation($"Finding certificate - Thumbprint:{_ThumbprintConfig.Thumbprint}, RootTrusted:{_ThumbprintConfig.RootTrusted}.");

            var result = store.Certificates
                .Find(X509FindType.FindByThumbprint, _ThumbprintConfig.Thumbprint, _ThumbprintConfig.RootTrusted)
                .OfType<X509Certificate2>()
                .FirstOrDefault();

            if (result == null)
                _Logger.LogCritical($"Certificate not found: {_ThumbprintConfig.Thumbprint}"); //TODO throw exception?

            return result;
        }
    }
}