using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
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
            services.AddScoped<ManifestBatchJob>();
            services.AddScoped<DynamicManifestReader>();
            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ContentDbContext(builder.Build());
                return result;
            });
            services.AddScoped<ManifestBuilder>();
            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            if (configuration.GetValue("DevelopmentFlags:UseCertificatesFromResources", false))
            {
                services.AddScoped<IContentSigner>(x => new CmsSigner(new ResourceCertificateProvider3(new StandardCertificateLocationConfig(configuration, "Certificates:NL"))));
            }
            else
            {
                services.AddScoped<IContentSigner>(x
                    => new CmsSigner(new X509CertificateProvider(new CertificateProviderConfig(x.GetRequiredService<IConfiguration>(), "Certificates:NL"),
                        x.GetRequiredService<ILogger<X509CertificateProvider>>())));
            }
            services.AddScoped<IJsonSerializer, StandardJsonSerializer>();
            services.AddScoped<GetActiveExposureKeySetsListCommand>();
            services.AddScoped<GetLatestContentCommand<RiskCalculationContentEntity>>();
            services.AddScoped<GetLatestContentCommand<AppConfigContentEntity>>();
            services.AddScoped<IGaenContentConfig, GaenContentConfig>();
        }
    }
}