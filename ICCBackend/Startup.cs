// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Authentication;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend
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
            ComponentsContainerHelper.RegisterDefaultServices(services);

            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            // Database Scoping
            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Icc");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new IccBackendContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });
            services.AddScoped<IEfDbConfig>(x => new StandardEfDbConfig(Configuration, "Icc"));
            services.AddScoped<ProvisionDatabasesCommandIcc, ProvisionDatabasesCommandIcc>();
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddScoped<IBasicAuthenticationConfig, BasicAuthenticationConfig>();
            services.AddScoped<IIccService, IccService>();
            services.AddScoped<AppBackendService, AppBackendService>();
            services.AddAuthentication("IccAuthentication")
                .AddScheme<AuthenticationSchemeOptions, IccAuthenticationHandler>("IccAuthentication", null);

            services.AddCors();

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Dutch Exposure Notification ICC API (inc. dev support)",
                    Version = "v1",
                    Description = "This specification describes the interface between the Dutch exposure notification app backend, ICC Webportal and the ICC backend service.",
                    Contact = new OpenApiContact
                    {
                        Name = "Ministerie van Volksgezondheid Welzijn en Sport backend repository", //TODO looks wrong?
                        Url = new Uri("https://github.com/minvws/nl-covid19-notification-app-backend"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "European Union Public License v. 1.2",
                        //TODO this should be https://joinup.ec.europa.eu/collection/eupl/eupl-text-eupl-12
                        Url = new Uri("https://github.com/minvws/nl-covid19-notification-app-backend/blob/master/LICENSE.txt")
                    },

                });
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "NL.Rijksoverheid.ExposureNotification.BackEnd.Components.xml"));
                o.AddSecurityDefinition("Icc", new OpenApiSecurityScheme
                {
                    Description = "Icc Code Authentication",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "IccAuthentication"
                });
                o.OperationFilter<SecurityRequirementsOperationFilter>();

            });
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().WithExposedHeaders("Content-Disposition")); // TODO: Fix CORS
            
            app.UseSwagger();
            app.UseSwaggerUI(o => { o.SwaggerEndpoint("../swagger/v1/swagger.json", "Icc Back-end Server V1"); });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}