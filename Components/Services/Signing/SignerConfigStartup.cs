// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public static void NlSignerStartup(this IServiceCollection services)
        {
            services.AddTransient<IContentSigner>(x =>
                new CmsSignerEnhanced(
                    new LocalMachineStoreCertificateProvider(
                        new LocalMachineStoreCertificateProviderConfig(
                            x.GetRequiredService<IConfiguration>(), NlSettingPrefix),
                            x.GetRequiredService<ILogger<LocalMachineStoreCertificateProvider>>()),
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
                        x.GetRequiredService<ILogger<LocalMachineStoreCertificateProvider>>()
                    )));
        }

        public static void DummySignerStartup(this IServiceCollection services)
		{
            services.AddTransient<IContentSigner>(x => new DummyCmsSigner());
		}
    }
}