// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.ServiceRegistrations
{
    public static class SigningProcessSetup
    {
        public static void SigningProcessRegistration(this IServiceCollection services)
        {
            // Batch Job
            services.AddTransient<IIksSigner, EfgsCmsSigner>();
            services.AddTransient<IEmbeddedResourceCertificateConfig, EmbeddedResourceCertificateConfig>();
            services.AddTransient<ICertificateChainProvider, EmbeddedResourcesCertificateChainProvider>();
            services.AddTransient<ICertificateProvider>(
                x => new LocalMachineStoreCertificateProvider(
                    new LocalMachineStoreCertificateProviderConfig(
                        x.GetRequiredService<IConfiguration>(), "Certificates:EfgsSigning"),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()
                ));
        }
    }
}
