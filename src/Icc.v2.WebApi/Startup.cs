// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.Icc.v2.WebApi
{
    public class Startup
    {
        private const string Title = "GGD Portal Backend";

        private readonly bool _IsDev;
        private readonly IConfiguration _Configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _IsDev = env?.IsDevelopment() ?? throw new ArgumentException(nameof(env));
        }
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            });

            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            services.AddTransient<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();

            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = _Configuration.GetConnectionString(DatabaseConnectionStringNames.IccDistMemCache);
                options.SchemaName = "dbo";
                options.TableName = "Cache";
            });

            services.AddScoped(x => x.CreateDbContext(y => new WorkflowDbContext(y), DatabaseConnectionStringNames.Workflow));
            
            services.AddTransient<AuthorisationArgsValidator>();
            services.AddScoped<HttpPostAuthoriseLabConfirmationIdCommand>();
            services.AddTransient<AuthorizeLabConfirmationIdCommand>();

            services.AddSingleton<IIccPortalConfig, IccPortalConfig>();

            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddCors();

            if (_IsDev)
                services.AddSwaggerGen(o => { o.SwaggerDoc("v1", new OpenApiInfo { Title = Title, Version = "v1" }); });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }
        
        public void Configure(IApplicationBuilder app)
        {
            if (_IsDev)
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(o => { o.SwaggerEndpoint("v1/swagger.json", Title); });
            }

            // HttpsRedirection will be handled at the infrastructure level
            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}