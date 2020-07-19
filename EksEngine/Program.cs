// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var useAllKeys = args.Length > 0 && bool.TryParse(args[0], out var value) && value;
            using var job = serviceProvider.GetRequiredService<ExposureKeySetBatchJobMk2>();
            job.Execute(useAllKeys).GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            var _Configuration = configuration; //Temp hack before extension method.

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ContentDbContext(builder.Build());
                return result;
            });

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped(x =>
                new ExposureKeySetBatchJobMk2(
                    x.GetRequiredService<IGaenContentConfig>(),
                    x.GetRequiredService<IExposureKeySetBuilder>(),
                    x.GetRequiredService<WorkflowDbContext>(),
                    x.GetRequiredService<ContentDbContext>(),
                    x.GetRequiredService<IUtcDateTimeProvider>(),
                    x.GetRequiredService<IPublishingId>(),
                    x.GetRequiredService<ILogger<ExposureKeySetBatchJobMk2>>()
                ));

            services.AddSingleton<IGaenContentConfig, StandardGaenContentConfig>();
            services.AddScoped<IExposureKeySetHeaderInfoConfig, ExposureKeySetHeaderInfoConfig>();
            services.AddScoped<IPublishingId, StandardPublishingIdFormatter>();

            if (_Configuration.GetValue("DevelopmentFlags:UseCertificatesFromResources", true))
            {
                if (_Configuration.GetValue("DevelopmentFlags:Azure", false))
                {
                    //AZURE
                    services.AddScoped<IExposureKeySetBuilder>(x =>
                        new ExposureKeySetBuilderV1(
                            x.GetRequiredService<IExposureKeySetHeaderInfoConfig>(),
                            new EcdSaSigner(new AzureResourceCertificateProvider(new StandardCertificateLocationConfig(_Configuration, "Certificates:GA"))),
                            new CmsSigner(new AzureResourceCertificateProvider(new StandardCertificateLocationConfig(_Configuration, "Certificates:NL"))),
                            x.GetRequiredService<IUtcDateTimeProvider>(), //TODO pass in time thru execute
                            new GeneratedProtobufContentFormatter(),
                            x.GetRequiredService<ILogger<ExposureKeySetBuilderV1>>()
                        ));

                    services.AddScoped<IContentSigner>(x => new CmsSigner(new AzureResourceCertificateProvider(new StandardCertificateLocationConfig(_Configuration, "Certificates:NL"))));
                }
                else
                {
                    //UNIT TESTS, LOCAL DEBUG
                    services.AddScoped<IExposureKeySetBuilder>(x =>
                        new ExposureKeySetBuilderV1(
                            x.GetRequiredService<IExposureKeySetHeaderInfoConfig>(),
                            new EcdSaSigner(new LocalResourceCertificateProvider(new StandardCertificateLocationConfig(x.GetRequiredService<IConfiguration>(), "Certificates:GA"))),
                            new CmsSigner(new LocalResourceCertificateProvider(new StandardCertificateLocationConfig(x.GetRequiredService<IConfiguration>(), "Certificates:NL"))),
                            x.GetRequiredService<IUtcDateTimeProvider>(), //TODO pass in time thru execute
                            new GeneratedProtobufContentFormatter(),
                            x.GetRequiredService<ILogger<ExposureKeySetBuilderV1>>()
                        ));

                    services.AddScoped<IContentSigner>(x => new CmsSigner(new LocalResourceCertificateProvider(new StandardCertificateLocationConfig(_Configuration, "Certificates:NL"))));
                }
            }
            else
            {
                //PROD
                services.AddScoped<IExposureKeySetBuilder>(x =>
                    new ExposureKeySetBuilderV1(
                        x.GetRequiredService<IExposureKeySetHeaderInfoConfig>(),
                        new EcdSaSigner(new X509CertificateProvider(new CertificateProviderConfig(x.GetRequiredService<IConfiguration>(), "Certificates:GA"), x.GetRequiredService<ILogger<X509CertificateProvider>>())),
                        new CmsSigner(new X509CertificateProvider(new CertificateProviderConfig(x.GetRequiredService<IConfiguration>(), "Certificates:NL"), x.GetRequiredService<ILogger<X509CertificateProvider>>())),
                        x.GetRequiredService<IUtcDateTimeProvider>(), //TODO pass in time thru execute
                        new GeneratedProtobufContentFormatter(),
                        x.GetRequiredService<ILogger<ExposureKeySetBuilderV1>>()
                    ));

                services.AddScoped<IContentSigner>(x => new CmsSigner(new X509CertificateProvider(new CertificateProviderConfig(x.GetRequiredService<IConfiguration>(), "Certificates:NL"), x.GetRequiredService<ILogger<X509CertificateProvider>>())));
            }








        }
    }
}
