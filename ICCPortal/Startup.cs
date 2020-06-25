// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheIdentityHub.AspNetCore.Authentication;

namespace IccPortal
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
            services.AddRazorPages()
                .AddRazorRuntimeCompilation();
            
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


            // services.AddMvc(options =>
            // {
            //     var policy = new AuthorizationPolicyBuilder()
            //         .AddAuthenticationSchemes(TheIdentityHubDefaults.AuthenticationScheme)
            //         .RequireAuthenticatedUser()
            //         .Build();
            //     options.Filters.Add(new AuthorizeFilter(policy));
            // });

            // services.AddSpaStaticFiles(configuration =>
            // {
            //     configuration.RootPath = "ClientApp/dist";
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseHsts();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                // endpoints.MapControllers();
                // endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            // app.UseMvc();

            // app.UseSpa(spa =>
            // {
            //     // To learn more about options for serving an Angular SPA from ASP.NET Core,
            //     // see https://go.microsoft.com/fwlink/?linkid=864501
            //
            //     spa.Options.SourcePath = "ClientApp";
            //
            //     if (env.IsDevelopment())
            //     {
            //         spa.UseAngularCliServer(npmScript: "start");
            //     }
            // });
        }
    }
}