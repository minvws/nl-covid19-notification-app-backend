// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing.Providers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsDownloader
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
            catch(Exception e)
            {
                return -1;
            }
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var config = serviceProvider.GetService<IEfgsConfig>();

            if (!config.DownloaderEnabled)
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                logger.LogWarning("EfgsDownloader is disabled by the configuration.");

                return;
            }

            var pollingBatchJob = serviceProvider.GetService<IksPollingBatchJob>();
            pollingBatchJob.ExecuteAsync().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddTransient(x => x.CreateDbContext(y => new IksInDbContext(y), DatabaseConnectionStringNames.IksIn, false));

            services.AddSingleton<IEfgsConfig, EfgsConfig>();
            services.AddTransient<IIHttpGetIksCommand, HttpGetIksCommand>();
            services.AddTransient<IIksWriterCommand, IksWriterCommand>();
            services.AddTransient<Func<IIHttpGetIksCommand>>(x => x.GetService<IIHttpGetIksCommand>);
            services.AddTransient<Func<IIksWriterCommand>>(x => x.GetService<IIksWriterCommand>);
            services.AddTransient<IksPollingBatchJob>();

            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate();

            services.AddSingleton<IThumbprintConfig, LocalMachineStoreCertificateProviderConfig>();
            
            services.AddTransient<LocalMachineStoreCertificateProviderLoggingExtensions>();
            services.AddTransient<IAuthenticationCertificateProvider>(x =>
                new LocalMachineStoreCertificateProvider(
                    new LocalMachineStoreCertificateProviderConfig(x.GetRequiredService<IConfiguration>(), "Certificates:EfgsAuthentication"),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()
                )
            );
        }
    }
}
