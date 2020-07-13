using System;
using System.Net;
using CdnDataReceiver.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.MvcHooks;
using Serilog;

namespace CdnDataReceiver2
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        private readonly IConfiguration _Configuration;
        private readonly IWebHostEnvironment _Environment;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSeriLog(_Configuration);
            services.AddMvc(options => options.Filters.Add(new SerilogServiceExceptionInterceptor(Log.Logger)));

            Log.Logger.Information($"Active environment name: {_Environment.EnvironmentName}"); //TODO obsolete?

            var certificateHack = _Configuration.GetValue("CertificateHack", false);
            if (certificateHack)
            {
                Log.Warning("Unproven hack for self-signed certificates is enabled.");
                ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) =>
                    true;
            }

            services.AddControllers();
            services.AddScoped<HttpPostContentReceiver2>();
            services.AddScoped<IBlobWriter, StandardBlobWriter>();
            services.AddScoped<ManifestBlobWriter>();
            services.AddSingleton<IStorageAccountConfig>(new StorageAccountAppSettings(_Configuration, "Local"));
            services.AddSingleton<IJsonSerializer, StandardJsonSerializer>();
            services.AddSingleton<IContentPathProvider>(new ContentPathProvider(_Configuration));

            //Queues
            var regionSyncEnabled = _Configuration.GetValue("RegionSync", true);
            if (regionSyncEnabled)
            {
                Log.Information($"Writing to queue for sync across regions is enabled: true.");
                services.AddScoped<IQueueSender<StorageAccountSyncMessage>, QueueSendCommand<StorageAccountSyncMessage>>();
                services.AddSingleton<IServiceBusConfig>(new ServiceBusConfig(_Configuration, "ServiceBus"));
            }
            else
            {
                Log.Warning($"Writing to queue for sync across regions is enabled: false.");
                services.AddScoped<IQueueSender<StorageAccountSyncMessage>, NotAQueueSender<StorageAccountSyncMessage>>();
            }

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Azure CDN Data Receiver 2 API",
                    Version = "v1",
                });
            });
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.ConfigObject.ShowExtensions = true;
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure CDN Data Receiver 2 API");
            });

        }
    }

}
