// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                new ConsoleAppRunner().Execute(args, Configure, Start);
                return 0;
            }
            catch(Exception)
            {
                return -1;
            }
        }

        private static void Start(IServiceProvider services, string[] args)
        {
            var job = services.GetRequiredService<ManifestUpdateCommand>();
            job.ExecuteAllAsync().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            // Db and basic functions
            services.AddTransient(x => x.CreateDbContext(y => new ContentDbContext(y), DatabaseConnectionStringNames.Content, false));
            services.AddTransient<Func<ContentDbContext>>(x => x.GetRequiredService<ContentDbContext>);
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IEksConfig, StandardEksConfig>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();

            // Orchestrating components
            services.AddTransient<ManifestUpdateCommand>();
            services.AddTransient<ManifestV2Builder>();
            services.AddTransient<ManifestV3Builder>();
            services.AddTransient<ManifestV4Builder>();

            // Operating components
            services.AddTransient<Func<IContentEntityFormatter>>(x => x.GetRequiredService<StandardContentEntityFormatter>);
            services.AddTransient<StandardContentEntityFormatter>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient(x => 
                SignerConfigStartup.BuildEvSigner(
                    x.GetRequiredService<IConfiguration>(),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>(),
                    x.GetRequiredService<IUtcDateTimeProvider>()));

            // Logging
            services.AddSingleton<ManifestUpdateCommandLoggingExtensions>();
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();
        }
    }
}
