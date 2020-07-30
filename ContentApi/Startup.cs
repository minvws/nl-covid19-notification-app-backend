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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentApi
{
    public class Startup
    {
        private const string Title = "Cdn Content Provider";

        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private readonly IConfiguration _Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();

            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(_Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddTransient<IHttpResponseHeaderConfig, HttpResponseHeaderConfig>();
            services.AddSingleton<IUtcDateTimeProvider>(new StandardUtcDateTimeProvider());
            services.AddTransient<IPublishingId, StandardPublishingIdFormatter>();

            services.AddScoped<HttpGetCdnManifestCommand>();
            services.AddScoped<HttpGetCdnContentCommand>();

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo { Title = Title, Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("v1/swagger.json", Title);
            });


            if (env.IsDevelopment() || env.IsEnvironment("Test"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes
            }

            app.UseSerilogRequestLogging();
            
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
