// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.AspNet.DataProtection.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Handlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Validators;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.Icc.WebApi.Services;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.Icc.WebApi
{
    public class Startup
    {
        private const string Title = "GGD Portal Backend V1";

        private readonly bool _IsDev;
        private readonly bool _UseTestJwtClaims;
        private readonly IConfiguration _Configuration;
        private readonly IWebHostEnvironment _WebHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _WebHostEnvironment = env;
            _IsDev = env?.IsDevelopment() ?? throw new ArgumentException(nameof(env));
            _UseTestJwtClaims = !env.IsProduction();
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

            services.AddTransient<IAuthCodeGenerator, AuthCodeGenerator>();
            services.AddSingleton<IAuthCodeService, AuthCodeService>();

            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = _Configuration.GetConnectionString(DatabaseConnectionStringNames.IccDistMemCache);
                options.SchemaName = "dbo";
                options.TableName = "Cache";
            });

            services.AddScoped(x => x.CreateDbContext(y => new DataProtectionKeysDbContext(y), DatabaseConnectionStringNames.DataProtectionKeys));
            services.AddScoped(x => x.CreateDbContext(y => new WorkflowDbContext(y), DatabaseConnectionStringNames.Workflow, false));
            
            services.AddDataProtection().PersistKeysToDbContext<DataProtectionKeysDbContext>();

            services.AddScoped<HttpPostAuthoriseCommand>();
            services.AddScoped<HttpGetLogoutCommand>();
            services.AddScoped<HttpGetUserClaimCommand>();
            services.AddScoped<HttpPostAuthorizationTokenCommand>();
            services.AddScoped<HttpGetAuthorisationRedirectCommand>();
            services.AddScoped<HttpGetAccessDeniedCommand>();


            services.AddSingleton<IIccPortalConfig, IccPortalConfig>();
            services.AddTransient<IPublishTekService, PublishTekService>();
            services.AddTransient<PublishTekArgsValidator>();
            services.AddTransient<PublishTekCommand>();
            services.AddTransient<ILuhnModNConfig, LuhnModNConfig>();
            services.AddTransient<ILuhnModNValidator, LuhnModNValidator>();
            services.AddTransient<ILuhnModNGenerator, LuhnModNGenerator>();

            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<AuthorisationArgsValidator>();
            services.AddTransient<AuthorisationWriterCommand>();


            services.AddTransient<IJwtService, JwtService>();

            if (_UseTestJwtClaims)
            {
                services.AddTransient<IJwtClaimValidator, TestJwtClaimValidator>();
                services.AddSingleton<TestJwtGeneratorService>();
            }
            else
            {
                services.AddTransient<IJwtClaimValidator, JwtClaimValidator>();
            }

            services.AddTransient<WriteNewPollTokenWriter>();
            services.AddTransient<IPollTokenService, PollTokenService>();
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddCors();

            if (_IsDev)
                services.AddSwaggerGen(StartupSwagger);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            StartupIdentityHub(services);

            StartupAuthenticationScheme(services.AddAuthentication(JwtAuthenticationHandler.SchemeName));
        }

        private void StartupSwagger(SwaggerGenOptions o)
        {
            o.SwaggerDoc("v1", new OpenApiInfo {Title = Title, Version = "v1"});

            o.OperationFilter<SecurityRequirementsOperationFilter>();

            o.AddSecurityDefinition("Icc", new OpenApiSecurityScheme
            {
                Description = "Icc Code Authentication",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "IccAuthentication"
            });
        }

        private void StartupAuthenticationScheme(AuthenticationBuilder authBuilder)
        {
            authBuilder.AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(
                JwtAuthenticationHandler.SchemeName, null);
        }

        private void StartupIdentityHub(IServiceCollection services)
        {
            var iccIdentityHubConfig = new IccIdentityHubConfig(_Configuration);

            services
                .AddAuthentication(auth =>
                {
                    auth.DefaultChallengeScheme = TheIdentityHubDefaults.AuthenticationScheme;
                    auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddTheIdentityHubAuthentication(options =>
                {
                    options.TheIdentityHubUrl = new Uri(iccIdentityHubConfig.BaseUrl);
                    options.Tenant = iccIdentityHubConfig.Tenant;
                    options.ClientId = iccIdentityHubConfig.ClientId;
                    options.ClientSecret = iccIdentityHubConfig.ClientSecret;
                    options.CallbackPath = iccIdentityHubConfig.CallbackPath;
                });


            var iccPortalConfig = new IccPortalConfig(_Configuration);

            var policyAuthorizationOptions = new PolicyAuthorizationOptions(_WebHostEnvironment, iccPortalConfig);
            services.AddAuthorization(policyAuthorizationOptions.Build);

            services.AddTransient<ITheIdentityHubService, TheIdentityHubService>();

            services.AddMvc(config =>
            {
                config.EnableEndpointRouting = false;
                var policy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(TheIdentityHubDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_IsDev)
            {
                app.ApplicationServices.GetService<TestJwtGeneratorService>();
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(o => { o.SwaggerEndpoint("v1/swagger.json", Title); });
            }

            app.UseForwardedHeaders();

            if (app == null) throw new ArgumentNullException(nameof(app));

            var iccPortalConfig = new IccPortalConfig(_Configuration);

            var corsOptions = new CorsOptions(iccPortalConfig);
            app.UseCors(corsOptions.Build);

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

            // HttpsRedirection will be handled at the infrastructure level
            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCookiePolicy();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}