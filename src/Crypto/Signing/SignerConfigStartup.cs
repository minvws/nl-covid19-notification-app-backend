// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    public static class SignerConfigStartup
    {
        private const string GaSettingPrefix = "Certificates:GA";
        private const string GaV15SettingPrefix = "Certificates:GAv15";
        private const string NlSettingPrefix = "Certificates:NL";
        private const string NlChainPrefix = NlSettingPrefix + ":Chain";

        public static IContentSigner BuildEvSigner(
            IConfiguration config,
            ILogger<LocalMachineStoreCertificateProvider> logger,
            IUtcDateTimeProvider dateTimeProvider)
        {
            return new CmsSignerEnhanced(
                new LocalMachineStoreCertificateProvider(logger),
                new CertificateChainConfig(config, NlChainPrefix),
                dateTimeProvider,
                new ThumbprintConfig(
                    config,
                    NlSettingPrefix)
                );
        }

        public static IGaContentSigner BuildGaSigner(
            ILogger<LocalMachineStoreCertificateProvider> logger,
            IConfiguration config)
        {
            return new GASigner(
                new LocalMachineStoreCertificateProvider(logger),
                new ThumbprintConfig(
                    config,
                    GaSettingPrefix));
        }

        public static IGaContentSigner BuildGaV15Signer(
            ILogger<LocalMachineStoreCertificateProvider> logger,
            IConfiguration config)
        {
            return new GAv15Signer(
                new LocalMachineStoreCertificateProvider(logger),
                new ThumbprintConfig(
                    config,
                    GaV15SettingPrefix));
        }
    }
}
