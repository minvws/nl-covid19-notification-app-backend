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
            job.ExecuteV1Async().GetAwaiter().GetResult();
            job.ExecuteV3Async().GetAwaiter().GetResult();
            job.ExecuteV4Async().GetAwaiter().GetResult();

            var job2 = services.GetRequiredService<NlContentResignExistingV1ContentCommand>();
            job2.ExecuteAsync().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient(x => x.CreateDbContext(y => new ContentDbContext(y), DatabaseConnectionStringNames.Content, false));
            services.AddTransient<Func<ContentDbContext>>(x => x.GetRequiredService<ContentDbContext>);

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped<HttpGetCdnManifestCommand>();
            services.AddScoped<HttpGetCdnContentCommand>();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddTransient<ManifestUpdateCommand>();
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<ManifestBuilder>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();

            services.AddSingleton<GetCdnContentLoggingExtensions>();
            services.AddSingleton<ResignerLoggingExtensions>();
            services.AddSingleton<ManifestUpdateCommandLoggingExtensions>();
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();

            services.NlResignerStartup();
            services.ManifestForV4Startup();
        }
    }
}