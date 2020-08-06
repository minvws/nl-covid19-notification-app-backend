using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using System;

namespace ManagementPortal
{
    public class Startup
    {
        private readonly IConfiguration _Configuration;
        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped(x => DbContextStartup.Workflow(x, false));
            services.AddScoped(x => DbContextStartup.Content(x, false));
            services.AddScoped(x => DbContextStartup.Publishing(x, false));

            services.AddTransient<ContentValidator>();
            services.AddTransient<ContentInsertDbCommand>();

            services.NlSignerStartup(_Configuration.UseCertificatesFromResources());
            services.GaSignerStartup(_Configuration.UseCertificatesFromResources());

            // Database Scoping
            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(_Configuration, "WorkFlow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<ZippedSignedContentFormatter>();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime,
            ILogger<Startup> logger)
        {
            if (!(env.IsDevelopment() || env.IsEnvironment("Test")))
            {
                logger.LogCritical(
                    $"Application environment {env.EnvironmentName} not allowed. Shutting down immediately!");
                lifetime.StopApplication();
            }
            else if (env.IsDevelopment())
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