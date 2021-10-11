// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public static void DummySignerStartup(this IServiceCollection services)
        {
            services.AddTransient<IContentSigner>(x => new DummyCmsSigner());
        }

        public static IContentSigner BuildEvSigner(
            IConfiguration config,
            LocalMachineStoreCertificateProviderLoggingExtensions loggingExtensions,
            IUtcDateTimeProvider dateTimeProvider)
        {
            return new CmsSignerEnhanced(
                new LocalMachineStoreCertificateProvider(loggingExtensions),
                new CertificateChainConfig(config, NlChainPrefix),
                dateTimeProvider,
                new ThumbprintConfig(
                    config,
                    GaSettingPrefix)
                );
        }

        public static IGaContentSigner BuildGaSigner(
            LocalMachineStoreCertificateProviderLoggingExtensions loggingExtensions,
            IConfiguration config)
        {
            return new GASigner(
                new LocalMachineStoreCertificateProvider(loggingExtensions),
                new ThumbprintConfig(
                    config,
                    GaSettingPrefix));
        }

        public static IGaContentSigner BuildGaV15Signer(
            LocalMachineStoreCertificateProviderLoggingExtensions loggingExtensions,
            IConfiguration config)
        {
            return new GAv15Signer(
                new LocalMachineStoreCertificateProvider(loggingExtensions),
                new ThumbprintConfig(
                    config,
                    GaV15SettingPrefix));
        }
    }
}
