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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;

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
            var config = serviceProvider.GetService<IEfgsConfig>();

            if (!config.UploaderEnabled)
            {
                var logger = serviceProvider.GetService<IksUploaderLoggingExtensions>();
                logger.WriteDisabledByConfig();

                return;
            }

            var sendBatchCommand = serviceProvider.GetService<IksSendBatchCommand>();
            sendBatchCommand.ExecuteAsync().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient(x => x.CreateDbContext(y => new IksOutDbContext(y), DatabaseConnectionStringNames.IksOut, false));

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IEfgsConfig, EfgsConfig>();
            services.AddTransient<IksSendBatchCommand>();
            services.AddTransient<HttpPostIksCommand>();
            services.AddSingleton<Func<IksOutDbContext>>(x => x.GetService<IksOutDbContext>);
            services.AddSingleton<Func<HttpPostIksCommand>>(x => x.GetService<HttpPostIksCommand>);
            services.AddTransient<IBatchTagProvider, BatchTagProvider>();
            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();
            services.AddSingleton<IksUploaderLoggingExtensions>();
            
            // IKS Signing
            services.AddTransient<IIksSigner, EfgsCmsSigner>();
            services.AddTransient<IEmbeddedResourceCertificateConfig, EmbeddedResourceCertificateConfig>();
            services.AddTransient<ICertificateChainProvider, EmbeddedResourcesCertificateChainProvider>();
            services.AddTransient<ICertificateProvider>(
                x => new LocalMachineStoreCertificateProvider(
                    new LocalMachineStoreCertificateProviderConfig(
                        x.GetRequiredService<IConfiguration>(), "Certificates:EfgsSigning"),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()
                ));
            
            // Authentication (with certs)
            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate();
            services.AddTransient<IAuthenticationCertificateProvider>(x =>
                new LocalMachineStoreCertificateProvider(
                    new LocalMachineStoreCertificateProviderConfig(
                        x.GetRequiredService<IConfiguration>(), "Certificates:EfgsAuthentication"),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()
                ));
        }
    }
}
