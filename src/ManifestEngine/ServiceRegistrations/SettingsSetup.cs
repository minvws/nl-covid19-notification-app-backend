// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine.ServiceRegistrations
{
    public static class SettingsSetup
    {
        public static void SettingsRegistration(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();

            services.AddSingleton<IHsmSignerConfig, HsmSignerConfig>(
                x => new HsmSignerConfig(configuration, "Certificates:HsmSigner")
            );
        }
    }
}
