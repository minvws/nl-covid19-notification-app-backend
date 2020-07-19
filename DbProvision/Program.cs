// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.GenericContent;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace DbProvision
{
    class Program
    {
        static void Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
        }

        private static void Start(IServiceProvider services, string[] _)
        {
            services.GetRequiredService<ProvisionDatabasesCommand>().Execute().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ContentDbContext(builder.Build());
                return result;
            });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(configuration, "Workflow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                return result;
            });

            services.AddTransient<CreateWorkflowDatabase>();
            services.AddTransient<CreateContentDatabase>();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddScoped<ProvisionDatabasesCommand>();
            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IGaenContentConfig, StandardGaenContentConfig>();
            services.AddScoped<IPublishingId, StandardPublishingIdFormatter>();
            services.AddScoped<GenericContentValidator>();
            services.AddScoped<ContentInsertDbCommand>();
            services.AddScoped<ZippedSignedContentFormatter>();

            if (configuration.GetValue("DevelopmentFlags:UseCertificatesFromResources", true))
            {
                if (configuration.GetValue("DevelopmentFlags:Azure", false))
                {
                    //AZURE
                    services.AddScoped<IContentSigner>(x => new CmsSigner(new AzureResourceCertificateProvider(new StandardCertificateLocationConfig(configuration, "Certificates:NL"))));
                }
                else
                {
                    //UNIT TESTS, LOCAL DEBUG
                    services.AddScoped<IContentSigner>(x => new CmsSigner(new LocalResourceCertificateProvider(new StandardCertificateLocationConfig(configuration, "Certificates:NL"))));
                }
            }
            else
            {
                services.AddScoped<IContentSigner>(x => new CmsSigner(new X509CertificateProvider(new CertificateProviderConfig(x.GetRequiredService<IConfiguration>(), "Certificates:NL"), x.GetRequiredService<ILogger<X509CertificateProvider>>())));
            }
        }
    }
}
