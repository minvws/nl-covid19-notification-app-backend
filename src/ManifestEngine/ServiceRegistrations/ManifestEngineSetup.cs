// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine.Jobs;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine.ServiceRegistrations
{
    public static class ManifestEngineSetup
    {
        public static void ManifestEngineRegistration(this IServiceCollection services)
        {
            // Orchestrating components
            services.AddTransient<ManifestBatchJob>();

            // Signing
            services.AddHttpClient<IHsmSignerService, HsmSignerService>();

            // Manifest commands
            services.AddTransient<ManifestUpdateCommand>();
            services.AddTransient<ManifestBuilder>();

            // Operating components
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
        }
    }
}
