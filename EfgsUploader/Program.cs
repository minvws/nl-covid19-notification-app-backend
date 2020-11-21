// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader
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

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var sendBatchCommand = serviceProvider.GetService<IksSendBatchCommand>();
            sendBatchCommand.ExecuteAsync().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient(x => DbContextStartup.IksIn(x));
            services.AddSingleton<IEfgsConfig, EfgsConfig>();
            services.AddTransient<IksSendBatchCommand>();
            services.AddTransient<HttpPostIksCommand>();
            services.AddSingleton<Func<IksOutDbContext>>(x => x.GetService<IksOutDbContext>);
            services.AddSingleton<Func<HttpPostIksCommand>>(x => x.GetService<HttpPostIksCommand>);
            services.AddTransient<IBatchTagProvider, BatchTagProvider>();
            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient(_ => DbContextStartup.IksOut(_, false));

            // IKS Signing
            services.AddTransient<IIksSigner, EfgsCmsSigner>();
            services.AddTransient<ICertificateLocationConfig, StandardCertificateLocationConfig>();
            services.AddTransient<ICertificateChainProvider, EmbeddedResourcesCertificateChainProvider>();
            services.AddTransient<ICertificateProvider>(x =>
                new LocalMachineStoreCertificateProvider(
                    new LocalMachineStoreCertificateProviderConfig(x.GetRequiredService<IConfiguration>(), "Certificates:EfgsSigning"),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()
                )
            );
            
            // Authentication (with certs)
            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate();
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
