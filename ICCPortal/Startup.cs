using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
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
            
            // TODO @hidde: Make Authentication setup final 
            // services.AddAuthentication(auth =>
            //     {
            //         auth.DefaultChallengeScheme = TheIdentityHubDefaults.AuthenticationScheme;
            //         auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //         auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //     }).AddCookie()
            //     .AddTheIdentityHubAuthentication(options =>
            //     {
            //         options.TheIdentityHubUrl = new Uri("https://www.theidentityhub.com");
            //         options.Tenant = Configuration.GetSection("IccPortalConfig:IdentityHub:tenant").Value;
            //         options.ClientId = Configuration.GetSection("IccPortalConfig:IdentityHub:client_id").Value;
            //         options.ClientSecret = Configuration.GetSection("IccPortalConfig:IdentityHub:client_secret").Value;
            //     });
            //
            //
            // services.AddMvc(options =>
            // {
            //     var policy = new AuthorizationPolicyBuilder()
            //         .AddAuthenticationSchemes(TheIdentityHubDefaults.AuthenticationScheme)
            //         .RequireAuthenticatedUser()
            //         .Build();
            //     options.Filters.Add(new AuthorizeFilter(policy));
            // });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
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
            // app.UseAuthorization();
            // app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            // app.UseMvc();

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