// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
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
            services.AddTransient<ManifestBuilder>();
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
        }
    }
}
