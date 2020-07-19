// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
        }

        private static void Start(IServiceProvider services, string[] args)
        {
            services.GetRequiredService<ManifestBatchJob>().Execute().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddLogging();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<ManifestBatchJob>();
            services.AddTransient<ManifestBuilderAndFormatter>();
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<IPublishingId, StandardPublishingIdFormatter>();

            services.AddTransient<HttpGetCdnManifestCommand>();
            services.AddTransient<HttpGetCdnGenericContentCommand>();
            services.AddTransient<HttpGetCdnContentCommand<ExposureKeySetContentEntity>>();

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ContentDbContext(builder.Build());
                return result;
            });
            services.AddTransient<ManifestBuilder>();
            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            if (configuration.GetValue("DevelopmentFlags:UseCertificatesFromResources", false))
            {
                if (configuration.GetValue("DevelopmentFlags:Azure", false))
                    services.AddTransient<IContentSigner>(x => new CmsSigner(new AzureResourceCertificateProvider(new StandardCertificateLocationConfig(configuration, "Certificates:NL"))));
                else
                    services.AddTransient<IContentSigner>(x => new CmsSigner(new LocalResourceCertificateProvider(new StandardCertificateLocationConfig(configuration, "Certificates:NL"))));
            }
            else
            {
                services.AddTransient<IContentSigner>(x
                    => new CmsSigner(new X509CertificateProvider(new CertificateProviderConfig(x.GetRequiredService<IConfiguration>(), "Certificates:NL"),
                        x.GetRequiredService<ILogger<X509CertificateProvider>>())));
            }
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddSingleton<IGaenContentConfig, StandardGaenContentConfig>();
        }
    }
}