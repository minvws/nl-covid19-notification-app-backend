// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.CdnDataApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSeriLog(Configuration);

            services.AddControllers();

            services.AddScoped<HttpGetManifestBinaryContentCommand, HttpGetManifestBinaryContentCommand>();
            services.AddScoped<DynamicManifestReader, DynamicManifestReader>();
            services.AddScoped<ManifestBuilder, ManifestBuilder>();
            services.AddSingleton<IUtcDateTimeProvider>(new StandardUtcDateTimeProvider());
            services.AddSingleton<IPublishingId>(new StandardPublishingIdFormatter());
            services.AddScoped<IContentSigner, FakeContentSigner>();
            services.AddSingleton<IGaenContentConfig>(new GaenContentConfig(Configuration));

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ExposureContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddScoped<GetActiveExposureKeySetsListCommand, GetActiveExposureKeySetsListCommand>();
            services.AddScoped<GetLatestContentCommand<ResourceBundleContentEntity>, GetLatestContentCommand<ResourceBundleContentEntity>>();
            services.AddScoped<GetLatestContentCommand<RiskCalculationContentEntity>, GetLatestContentCommand<RiskCalculationContentEntity>>();
            services.AddScoped<GetLatestContentCommand<AppConfigContentEntity>, GetLatestContentCommand<AppConfigContentEntity>>();


            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CDN Data API",
                    Version = "v1",
                    Description = "///",
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.ConfigObject.ShowExtensions = true;
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "CDN Data API");
            });
            app.UseHttpsRedirection();
            app.UseRouting();
            //app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
