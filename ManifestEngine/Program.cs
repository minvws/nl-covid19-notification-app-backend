// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine
{
    internal class Program
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
            services.GetRequiredService<ManifestUpdateCommand>().Execute().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped(x => DbContextStartup.Content(x, false));
            services.AddScoped<Func<ContentDbContext>>(x => x.GetRequiredService<ContentDbContext>);
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

            services.NlSignerStartup();
        }
    }
}