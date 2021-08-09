// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.ServiceRegistrations
{
    public static class ManifestUpdateSetup
    {
        public static void ManifestUpdateRegistration(this IServiceCollection services)
        {
            // Batch Job
            services.AddTransient<ManifestBatchJob>();

            // ManifestEngine
            services.AddTransient<ManifestUpdateCommand>();
            services.AddTransient<ManifestV2Builder>();
            services.AddTransient<ManifestV3Builder>();
            services.AddTransient<ManifestV4Builder>();
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient(x =>
                SignerConfigStartup.BuildEvSigner(
                    x.GetRequiredService<IConfiguration>(),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>(),
                    x.GetRequiredService<IUtcDateTimeProvider>()));
            services.AddTransient(x =>
                SignerConfigStartup.BuildGaSigner(
                    x.GetRequiredService<IConfiguration>(),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()));
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
        }
    }
}
