// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp.Extensions;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp
{
    public class Startup
    {
        private readonly bool _isDev;
        private readonly bool _useTestJwtClaims;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _webHostEnvironment = env;
            _isDev = env?.IsDevelopment() ?? throw new ArgumentException(nameof(env));
            _useTestJwtClaims = !env.IsProduction();
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

            services.AddScoped<HttpGetLogoutCommand>();
            services.AddScoped<HttpGetUserClaimCommand>();
            services.AddScoped<HttpPostAuthorizationTokenCommand>();
            services.AddScoped<HttpGetAuthorisationRedirectCommand>();
            services.AddScoped<HttpGetAccessDeniedCommand>();

            services.AddSingleton<IIccPortalConfig, IccPortalConfig>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<ILuhnModNConfig, LuhnModNConfig>();
            services.AddTransient<ILuhnModNValidator, LuhnModNValidator>();
            services.AddTransient<ILuhnModNGenerator, LuhnModNGenerator>();

            services.AddTransient<IJwtService, JwtService>();
            
            services.AddSingleton<IAuthCodeService, AuthCodeService>();
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddCors();

            if (_useTestJwtClaims)
            {
                services.AddTransient<IJwtClaimValidator, TestJwtClaimValidator>();
                services.AddSingleton<TestJwtGeneratorService>();
            }
            else
            {
                services.AddTransient<IJwtClaimValidator, JwtClaimValidator>();
            }

            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = _configuration.GetConnectionString(DatabaseConnectionStringNames.IccDistMemCache);
                options.SchemaName = "dbo";
                options.TableName = "Cache";
            });

            services.AddTransient(x => x.CreateDbContext(y => new WorkflowDbContext(y), DatabaseConnectionStringNames.Workflow));
            services.AddTransient<WriteNewPollTokenWriter>();
            services.AddTransient<IPollTokenService, PollTokenService>();

            StartupIdentityHub(services);
            StartupAuthenticationScheme(services.AddAuthentication(JwtAuthenticationHandler.SchemeName));
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
            app.UseAuthentication();
            app.UseAuthorization();
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

        private void StartupAuthenticationScheme(AuthenticationBuilder authBuilder)
        {
            authBuilder.AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(
                JwtAuthenticationHandler.SchemeName, null);
        }
        private void StartupIdentityHub(IServiceCollection services)
        {
            var iccIdentityHubConfig = new IccIdentityHubConfig(_configuration);

            services.AddAuthentication(auth =>
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


            var iccPortalConfig = new IccPortalConfig(_configuration);

            var policyAuthorizationOptions = new PolicyAuthorizationOptions(_webHostEnvironment, iccPortalConfig);
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
    }
}
