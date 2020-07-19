// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Authorization;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Services;
using System;
using System.IO;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend
{
    public class Startup
    {
        private string BaseUrl
        {
            get
            {
                var configSection = _Configuration.GetSection("IccPortalConfig:IdentityHub:base_url");

                return configSection.Exists() ? configSection.Value : string.Empty;
            }
        }

        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private readonly IConfiguration _Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddLogging();

            services.AddScoped<IJsonSerializer, StandardJsonSerializer>();

            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            // Database Scoping
            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(_Configuration, "WorkFlow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddScoped<FrontendService, FrontendService>();
            services.AddScoped<HttpPostAuthorise, HttpPostAuthorise>();
            services.AddScoped<HttpPostLabVerify, HttpPostLabVerify>();
            services.AddScoped<IAuthorisationWriter, AuthorisationWriter>();
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddScoped<JwtService, JwtService>();
            services.AddScoped<LabVerifyChecker, LabVerifyChecker>();
            services.AddScoped<PollTokenGenerator, PollTokenGenerator>();

            services.AddMvc(config =>
            {
                config.EnableEndpointRouting = false;
                var policy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(TheIdentityHubDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
                // .RequireClaim(ClaimTypes.Role)
            });

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

                        services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // TODO: Make service for adding authentication + configuration model
            services
                .AddAuthentication(auth => {
                    auth.DefaultChallengeScheme = TheIdentityHubDefaults.AuthenticationScheme;
                    auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddTheIdentityHubAuthentication(options =>
                {
                    if (!string.IsNullOrWhiteSpace(BaseUrl))
                    {
                        options.TheIdentityHubUrl = new Uri(BaseUrl);
                    }

                    options.Tenant = _Configuration.GetSection("IccPortalConfig:IdentityHub:tenant").Value;
                    options.ClientId = _Configuration.GetSection("IccPortalConfig:IdentityHub:client_id").Value;
                    options.ClientSecret = _Configuration.GetSection("IccPortalConfig:IdentityHub:client_secret").Value;
                });

            services
                .AddAuthentication("jwt")
                .AddScheme<AuthenticationSchemeOptions, JwtAuthorizationHandler>("jwt", null);
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (env == null) throw new ArgumentNullException(nameof(env));

            app.UseCors(options => 
                options.AllowAnyOrigin().AllowAnyHeader().WithExposedHeaders("Content-Disposition")); // TODO: Fix CORS
            
            app.UseSwagger();
            app.UseSwaggerUI(o => { o.SwaggerEndpoint("../swagger/v1/swagger.json", "GGD Portal Backend V1"); });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCookiePolicy();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}