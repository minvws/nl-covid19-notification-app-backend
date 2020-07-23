// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.GenericContent;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.BatchJobsApi
{
    public class Startup
    {
        private const string Title = "MSS EKSEngine Api";

        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private readonly IConfiguration _Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(_Configuration, "WorkFlow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                return result;
            });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(_Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ContentDbContext(builder.Build());
                return result;
            });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(_Configuration, "PublishingJob");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new PublishingJobDbContext(builder.Build());
                return result;
            });

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient(x =>
                new ExposureKeySetBatchJobMk2(
                    x.GetService<IGaenContentConfig>(),
                    x.GetService<IExposureKeySetBuilder>(),
                    x.GetService<WorkflowDbContext>(),
                    x.GetService<PublishingJobDbContext>(),
                    x.GetService<ContentDbContext>(),
                    x.GetService<IUtcDateTimeProvider>(),
                    x.GetService<IPublishingId>(),
                    x.GetService<ILogger<ExposureKeySetBatchJobMk2>>()
                ));

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

            services.AddScoped<IExposureKeySetHeaderInfoConfig, ExposureKeySetHeaderInfoConfig>();
            services.AddScoped<IPublishingId, StandardPublishingIdFormatter>();

            services.AddLogging();
            services.AddSingleton(_Configuration);
            services.AddTransient<ManifestBatchJob>();
            services.AddTransient<ManifestBuilder>();
            services.AddTransient<ManifestBuilderAndFormatter>();
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddSingleton<IGaenContentConfig, StandardGaenContentConfig>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddSingleton(_Configuration);

            services.AddTransient<WorkflowDatabaseCreateCommand>();
            services.AddTransient<PublishingJobDatabaseCreateCommand>();
            services.AddTransient<IccDatabaseCreateCommand>();
            services.AddTransient<ContentDatabaseCreateCommand>();
            services.AddTransient<ProvisionDatabasesCommand>();
            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient<IPublishingId, StandardPublishingIdFormatter>();
            services.AddTransient<GenericContentValidator>();
            services.AddTransient<ContentInsertDbCommand>();
            services.AddTransient<ZippedSignedContentFormatter>();

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo { Title = Title, Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("v1/swagger.json", Title);
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
