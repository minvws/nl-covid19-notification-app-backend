using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace CdnDataReceiver
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

            services.AddSeriLog(Configuration);

            services.AddControllers();

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ExposureContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddScoped<HttpPostContentReciever<ManifestEntity>, HttpPostContentReciever<ManifestEntity>>();
            services.AddScoped<HttpPostContentReciever<AppConfigContentEntity>, HttpPostContentReciever<AppConfigContentEntity>>();
            services.AddScoped<HttpPostContentReciever<ResourceBundleContentEntity>, HttpPostContentReciever<ResourceBundleContentEntity>>();
            services.AddScoped<HttpPostContentReciever<RiskCalculationContentEntity>, HttpPostContentReciever<RiskCalculationContentEntity>>();
            services.AddScoped<HttpPostContentReciever<ExposureKeySetContentEntity>, HttpPostContentReciever<ExposureKeySetContentEntity>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
