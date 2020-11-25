// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using System;

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
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return -1;
            }
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var pollingBatchJob = serviceProvider.GetService<IksPollingBatchJob>();
            pollingBatchJob.ExecuteAsync().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddTransient(x => DbContextStartup.IksIn(x, false));

            services.AddSingleton<IEfgsConfig, EfgsConfig>();
            services.AddTransient<HttpGetIksCommand>();
            services.AddTransient<IksWriterCommand>();
            services.AddTransient<Func<HttpGetIksCommand>>(x => x.GetService<HttpGetIksCommand>);
            services.AddTransient<Func<IksWriterCommand>>(x => x.GetService<IksWriterCommand>);
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
