using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.AspNet.DataProtection.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Handlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Validators;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.IccPortal
{
    public class Startup
    {
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


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddTransient<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddTransient<IAuthCodeGenerator, AuthCodeGenerator>();
            services.AddSingleton<IAuthCodeService, AuthCodeService>();

            services.AddScoped<HttpPostAuthoriseCommand>();
            services.AddScoped<HttpGetLogoutCommand>();
            services.AddScoped<HttpGetUserClaimCommand>();
            services.AddScoped<HttpPostAuthorizationTokenCommand>();
            services.AddScoped<HttpGetAuthorisationRedirectCommand>();
            services.AddScoped<HttpGetAccessDeniedCommand>();

            services.AddSingleton<IIccPortalConfig, IccPortalConfig>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<AuthorisationArgsValidator>();
            services.AddTransient<AuthorisationWriterCommand>();
            services.AddTransient<IJwtService, JwtService>();
            
            services.AddSingleton<IAuthCodeService, AuthCodeService>();
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddCors();

            if (_UseTestJwtClaims)
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
                options.ConnectionString = _Configuration.GetConnectionString(DatabaseConnectionStringNames.IccDistMemCache);
                options.SchemaName = "dbo";
                options.TableName = "Cache";
            });
            services.AddScoped(x => x.CreateDbContext(y => new DataProtectionKeysDbContext(y), DatabaseConnectionStringNames.DataProtectionKeys));
            services.AddScoped(x => x.CreateDbContext(y => new WorkflowDbContext(y), DatabaseConnectionStringNames.Workflow));

            services.AddDataProtection().PersistKeysToDbContext<DataProtectionKeysDbContext>();

            services.AddTransient<WriteNewPollTokenWriter>();
            services.AddTransient<IPollTokenService, PollTokenService>();

            StartupIdentityHub(services);
            StartupAuthenticationScheme(services.AddAuthentication(JwtAuthenticationHandler.SchemeName));
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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
            var iccIdentityHubConfig = new IccIdentityHubConfig(_Configuration);

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
    }
}
