// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.WebApi
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private const string Title = "Cdn Content Provider";

        private readonly bool _isDev;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env ?? throw new ArgumentException(nameof(env));
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            _isDev = env.IsDevelopment();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddDbContext<ContentDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString(DatabaseConnectionStringNames.Content)));

            services.AddScoped<HttpGetCdnManifestCommand>();
            services.AddScoped<HttpGetCdnEksCommand>();
            services.AddScoped<HttpGetCdnImmutableNonExpiringContentCommand>();
            services.AddScoped<HttpGetCdnDashboardDataCommand>();

            services.AddScoped<HttpGetCdnContentCommand>();
            services.AddTransient<EksCacheControlHeaderProcessor>();

            services.AddTransient<EksMaxageCalculator>();

            services.AddSingleton<IHttpResponseHeaderConfig, HttpResponseHeaderConfig>();
            services.AddSingleton<ITaskSchedulingConfig, StandardTaskSchedulingConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();

            if (_isDev)
            {
                services.AddSwaggerGen(o => { o.SwaggerDoc("v1", new OpenApiInfo { Title = Title, Version = "v1" }); });
            }

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.Always;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_isDev)
            {
                app.UseSwagger();
                app.UseSwaggerUI(o => { o.SwaggerEndpoint("v1/swagger.json", Title); });
                app.UseDeveloperExceptionPage();
            }

            var corsOptions = new CorsOptions(_env, new ContentApiConfig(_configuration));
            app.UseCors(corsOptions.Build);

            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
