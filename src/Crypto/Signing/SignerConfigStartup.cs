// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    public static class SignerConfigStartup
    {
        private const string NlSettingPrefix = "Certificates:NL";
        private const string GaSettingPrefix = "Certificates:GA";
        private const string ChainPrefix = NlSettingPrefix + ":Chain";
        private const string Nl2SettingPrefix = "Certificates:NL2";
        private const string Nl2ChainPrefix = Nl2SettingPrefix + ":Chain";

        public static void NlSignerStartup(this IServiceCollection services)
        {
            services.AddTransient<IContentSigner>(x =>
                new CmsSignerEnhanced(
                    new LocalMachineStoreCertificateProvider(
                        new LocalMachineStoreCertificateProviderConfig(
                            x.GetRequiredService<IConfiguration>(), NlSettingPrefix),
                            x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()),
                        new EmbeddedResourcesCertificateChainProvider(
                            new StandardCertificateLocationConfig(
                                x.GetRequiredService<IConfiguration>(),
                                ChainPrefix)),
                        x.GetRequiredService<IUtcDateTimeProvider>()
                    ));
        }

        public static void GaSignerStartup(this IServiceCollection services)
        {
            services.AddTransient<IGaContentSigner>(x =>
                new EcdSaSigner(
                    new LocalMachineStoreCertificateProvider(
                        new LocalMachineStoreCertificateProviderConfig(
                            x.GetRequiredService<IConfiguration>(),
                            GaSettingPrefix),
                        x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()
                    )));
        }

        public static void DummySignerStartup(this IServiceCollection services)
		{
            var lf = new LoggerFactory();
            var certProviderLogger = new EmbeddedCertProviderLoggingExtensions(lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());

            // Add hardcoded test cert to sign
            services.AddTransient<IContentSigner>(x =>
                new CmsSignerEnhanced(new EmbeddedResourceCertificateProvider(new HardCodedCertificateLocationConfig("TestRSA.p12", "Covid-19!"), certProviderLogger), //Not a secret.
                new EmbeddedResourcesCertificateChainProvider(new HardCodedCertificateLocationConfig("Resources.StaatDerNLChain-Expires2020-08-28.p7b", "")), //Not a secret.
                new StandardUtcDateTimeProvider()));
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
                        new StandardCertificateLocationConfig(
                            config,
                            Nl2ChainPrefix)),
                    dateTimeProvider
                );
        }
    }
}
