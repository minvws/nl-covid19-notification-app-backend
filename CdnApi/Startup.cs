// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content.NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ComponentsContainerHelper.RegisterDefaultServices(services);

            services.AddSeriLog(Configuration);

            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ExposureContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddSingleton<IUtcDateTimeProvider>(new StandardUtcDateTimeProvider());
            services.AddSingleton<IPublishingId, StandardPublishingIdFormatter>();

            services.AddScoped<HttpGetCdnManifestCommand, HttpGetCdnManifestCommand>();
            services.AddScoped<HttpGetCdnContentCommand<AppConfigContentEntity>, HttpGetCdnContentCommand<AppConfigContentEntity>>();
            services.AddScoped<HttpGetCdnContentCommand<ResourceBundleContentEntity>, HttpGetCdnContentCommand<ResourceBundleContentEntity>>();
            services.AddScoped<HttpGetCdnContentCommand<RiskCalculationContentEntity>, HttpGetCdnContentCommand<RiskCalculationContentEntity>>();
            services.AddScoped<HttpGetCdnContentCommand<ExposureKeySetContentEntity>, HttpGetCdnContentCommand<ExposureKeySetContentEntity>>();

            services.AddScoped<IReader<ManifestEntity>, SafeBinaryContentDbReader<ManifestEntity>>();
            services.AddScoped<IReader<AppConfigContentEntity>, SafeBinaryContentDbReader<AppConfigContentEntity>>();
            services.AddScoped<IReader<ResourceBundleContentEntity>, SafeBinaryContentDbReader<ResourceBundleContentEntity>>();
            services.AddScoped<IReader<RiskCalculationContentEntity>, SafeBinaryContentDbReader<RiskCalculationContentEntity>>();
            services.AddScoped<IReader<ExposureKeySetContentEntity>, SafeBinaryContentDbReader<ExposureKeySetContentEntity>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
