// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;

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
                var logger = serviceProvider.GetService<IksDownloaderLoggingExtensions>();
                logger.WriteDisabledByConfig();

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
            
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();
            services.AddSingleton<IksDownloaderLoggingExtensions>();

            services.AddTransient<IAuthenticationCertificateProvider>(
                x => new LocalMachineStoreCertificateProvider(
                    new LocalMachineStoreCertificateProviderConfig(
                        x.GetRequiredService<IConfiguration>(), "Certificates:EfgsAuthentication"),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()
                ));
        }
    }
}
