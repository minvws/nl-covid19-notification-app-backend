// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentApi
{
    public class Startup
    {
        private const string Title = "Cdn Content Provider";
        private readonly IConfiguration _Configuration;

        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped(x => DbContextStartup.Content(x));
            services.AddScoped<HttpGetCdnManifestCommand>();
            services.AddScoped<HttpGetCdnContentCommand>();

            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<IHttpResponseHeaderConfig, HttpResponseHeaderConfig>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();

            services.AddSwaggerGen(o => { o.SwaggerDoc("v1", new OpenApiInfo {Title = Title, Version = "v1"}); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()){
            
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(o => { o.SwaggerEndpoint("v1/swagger.json", Title); });
            }
            else
            {
                app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes
            }
            
            var corsOptions = new CorsOptions(env, new ContentApiConfig(_Configuration));
            app.UseCors(corsOptions.Build);
            
            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}