// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Claims;
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
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            // TODO: Make service for adding authentication + configuration model
            services.AddAuthentication(auth =>
                {
                    auth.DefaultChallengeScheme = TheIdentityHubDefaults.AuthenticationScheme;
                    auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                }).AddCookie()
                .AddTheIdentityHubAuthentication(options =>
                {
                    if (Configuration.GetSection("IccPortalConfig:IdentityHub:base_url")
                            .Exists() &&
                        !String.IsNullOrEmpty(Configuration.GetSection("IccPortalConfig:IdentityHub:base_url").Value))
                    {
                        options.TheIdentityHubUrl =
                            new Uri(Configuration.GetSection("IccPortalConfig:IdentityHub:base_url").Value);
                    }

                    options.Tenant = Configuration.GetSection("IccPortalConfig:IdentityHub:tenant").Value;
                    options.ClientId = Configuration.GetSection("IccPortalConfig:IdentityHub:client_id").Value;
                    options.ClientSecret = Configuration.GetSection("IccPortalConfig:IdentityHub:client_secret").Value;
                });
            services.AddAuthentication("jwt")
                .AddScheme<AuthenticationSchemeOptions, JwtAuthorizationHandler>("jwt", null);
            // services.AddAuthorization(options =>
            // {
            //     // options.AddPolicy("TelefonistRole",
            //     //     builder => builder.RequireClaim(ClaimTypes.Role, "C19NA-Telefonist-Test"));
            //     // options.AddPolicy("BeheerRole",
            //     //     builder => builder.RequireClaim(ClaimTypes.Role, "C19NA-Beheer-Test"));
            // });
            services.AddScoped<FrontendService, FrontendService>();
            services.AddScoped<JwtService, JwtService>();
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(options =>
                options.AllowAnyOrigin().AllowAnyHeader().WithExposedHeaders("Content-Disposition")); // TODO: Fix CORS

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}