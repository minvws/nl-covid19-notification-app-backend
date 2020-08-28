// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend
{
    public class Startup
    {
        private const string Title = "GGD Portal Backend V1";

        private readonly bool _IsDev;
        private readonly IConfiguration _Configuration;
        private readonly IWebHostEnvironment _WebHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _WebHostEnvironment = env;
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

            services.AddSingleton<IPaddingGenerator, CryptoRandomPaddingGenerator>();

            services.AddSingleton<IAuthCodeService, AuthCodeService>();
            services.AddDistributedMemoryCache();

            services.AddScoped(x => DbContextStartup.Workflow(x));
            services.AddScoped<HttpPostAuthoriseCommand>();
            services.AddScoped<HttpPostLabVerifyCommand>();
            services.AddScoped<HttpGetLogoutCommand>();
            services.AddScoped<HttpGetUserClaimCommand>();
            services.AddScoped<HttpPostAuthorizationTokenCommand>();
            services.AddScoped<HttpGetAuthorisationRedirectCommand>();
            services.AddScoped<HttpGetAccessDeniedCommand>();


            services.AddSingleton<IIccPortalConfig, IccPortalConfig>();

            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<AuthorisationArgsValidator>();
            services.AddTransient<LabVerifyArgsValidator>();
            services.AddTransient<AuthorisationWriterCommand>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();

            services.AddTransient<IJwtService, JwtService>();
            
            if (_IsDev)
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

            services.AddScoped<ITheIdentityHubService, TheIdentityHubService>();

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