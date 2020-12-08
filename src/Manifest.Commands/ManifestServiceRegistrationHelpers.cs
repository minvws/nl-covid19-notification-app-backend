// // Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// // Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// // SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ServiceRegHelpers
{
    public static class ManifestServiceRegistrationHelpers
    {
        public static void ManifestEngine(this IServiceCollection services)
        {
            services.AddTransient<ManifestUpdateCommand>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<ManifestBuilder>();
        }
    }
}