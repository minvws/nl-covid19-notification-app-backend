// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp.Extensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            });

            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            var iccPortalConfig = new IccPortalConfig(_configuration);
            services.AddRestApiClient(iccPortalConfig);

            services.AddTransient<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddTransient<IAuthCodeGenerator, AuthCodeGenerator>();
            services.AddSingleton<IAuthCodeService, AuthCodeService>();

            services.AddSingleton<IIccPortalConfig, IccPortalConfig>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<ILuhnModNConfig, LuhnModNConfig>();
            services.AddTransient<ILuhnModNValidator, LuhnModNValidator>();
            services.AddTransient<ILuhnModNGenerator, LuhnModNGenerator>();

            services.AddTransient<IJwtService, JwtService>();

            services.AddSingleton<IAuthCodeService, AuthCodeService>();
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddCors();
            
            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = _configuration.GetConnectionString(DatabaseConnectionStringNames.IccDistMemCache);
                options.SchemaName = "dbo";
                options.TableName = "Cache";
            });

            services.AddDbContext<WorkflowDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString(DatabaseConnectionStringNames.Workflow)));

            services.AddTransient<WriteNewPollTokenWriter>();
            services.AddTransient<IPollTokenService, PollTokenService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.Always;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.Use((context, next) =>
            {
                if (context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var protoHeaderValue))
                {
                    context.Request.Scheme = protoHeaderValue;
                }

                if (context.Request.Headers.TryGetValue("X-FORWARDED-HOST", out var hostHeaderValue))
                {
                    context.Request.Host = new HostString(hostHeaderValue);
                }

                return next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
