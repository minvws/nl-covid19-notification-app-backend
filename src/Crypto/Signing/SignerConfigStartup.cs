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
        private const string Nl2SettingPrefix = "Certificates:NL2";
        private const string Nl2ChainPrefix = Nl2SettingPrefix + ":Chain";

        public static void DummySignerStartup(this IServiceCollection services)
        {
            services.AddTransient<IContentSigner>(x => new DummyCmsSigner());
        }

        public static IContentSigner BuildEvSigner(
            IConfiguration config,
            LocalMachineStoreCertificateProviderLoggingExtensions extensions,
            IUtcDateTimeProvider dateTimeProvider)
        {
            return new CmsSignerEnhanced(
                new LocalMachineStoreCertificateProvider(
                    new LocalMachineStoreCertificateProviderConfig(
                        config,
                        Nl2SettingPrefix),
                    extensions),
                    new EmbeddedResourcesCertificateChainProvider(
                        new EmbeddedResourceCertificateConfig(
                            config,
                            Nl2ChainPrefix)),
                    dateTimeProvider);
        }

        public static IGaContentSigner BuildGaSigner(
            IConfiguration config,
            LocalMachineStoreCertificateProviderLoggingExtensions extensions)
        {
            return new EcdSaSigner(
                new LocalMachineStoreCertificateProvider(
                    new LocalMachineStoreCertificateProviderConfig(
                        config,
                        GaSettingPrefix),
                    extensions));
        }

    }
}
