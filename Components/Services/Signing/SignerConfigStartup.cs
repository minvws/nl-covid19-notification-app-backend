// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.LocalMachineStoreCertificateProvider;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing
{
    public static class SignerConfigStartup
    {
        private const string NlSettingPrefix = "Certificates:NL";
        private const string GaSettingPrefix = "Certificates:GA";
        private const string ChainPrefix = NlSettingPrefix + ":Chain";
        private const string EvSettingPrefix = "Certificates:NL2";

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

        //Injects a second CmsSignerEnhanced, but without an associated IContentSigner. Will be ignored by all classes but ZippedSignedContentFormatterForV3.
        public static void NlSignerForV3Startup(this IServiceCollection services)
        {
            services.AddTransient(x =>
                new CmsSignerEnhanced(
                    new LocalMachineStoreCertificateProvider(
                        new LocalMachineStoreCertificateProviderConfig(
                            x.GetRequiredService<IConfiguration>(), EvSettingPrefix),
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
            services.AddTransient<IContentSigner>(x => new DummyCmsSigner());
		}
    }
}