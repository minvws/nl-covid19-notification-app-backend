// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
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

            services.AddScoped<IJsonSerializer, StandardJsonSerializer>();

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

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped(x =>
                new ExposureKeySetBatchJobMk2(
                    x.GetService<IGaenContentConfig>(),
                    x.GetService<IExposureKeySetBuilder>(),
                    x.GetService<WorkflowDbContext>(),
                    x.GetService<ContentDbContext>(),
                    x.GetService<IUtcDateTimeProvider>(),
                    x.GetService<IPublishingId>(),
                    x.GetService<ILogger<ExposureKeySetBatchJobMk2>>()
                ));

            services.AddSingleton<IGaenContentConfig, StandardGaenContentConfig>();

            services.AddScoped<IExposureKeySetBuilder>(x =>
                new ExposureKeySetBuilderV1(
                    x.GetRequiredService<IExposureKeySetHeaderInfoConfig>(),
                    new EcdSaSigner(new ResourceCertificateProvider3(new StandardCertificateLocationConfig(x.GetRequiredService<IConfiguration>(), "Signing:GA"))),
                    new CmsSigner(new ResourceCertificateProvider3(new StandardCertificateLocationConfig(x.GetRequiredService<IConfiguration>(), "Signing:NL"))),
                    x.GetRequiredService<IUtcDateTimeProvider>(), //TODO pass in time thru execute
                    new GeneratedProtobufContentFormatter(),
                    x.GetRequiredService<ILogger<ExposureKeySetBuilderV1>>()
                ));

            services.AddScoped<IExposureKeySetHeaderInfoConfig, ExposureKeySetHeaderInfoConfig>();
            services.AddScoped<IPublishingId, StandardPublishingIdFormatter>();

            services.AddLogging();
            services.AddSingleton(_Configuration);
            services.AddScoped<ManifestBatchJob>();
            services.AddScoped<ManifestBuilder>();
            services.AddScoped<ManifestBuilderAndFormatter>();
            services.AddScoped<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddScoped<ZippedSignedContentFormatter>();

            services.AddScoped<IContentSigner>(x => new CmsSigner(new ResourceCertificateProvider3(new StandardCertificateLocationConfig(_Configuration, "Certificates:NL"))));
            services.AddScoped<IJsonSerializer, StandardJsonSerializer>();

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
