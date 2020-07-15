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
using Microsoft.Extensions.Logging;

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
        private bool _RegionSyncEnabled;

        /// <summary>
        /// NB. Cannot log in this method.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSeriLog(_Configuration);
            //services.AddMvc(options => options.Filters.Add(new SerilogServiceExceptionInterceptor(_Logger.Logger)));

            services.AddControllers();
            services.AddScoped<HttpPostContentReceiver2>();
            services.AddScoped<IBlobWriter, StandardBlobWriter>();
            services.AddScoped<ManifestBlobWriter>();
            services.AddSingleton<IStorageAccountConfig>(new StorageAccountAppSettings(_Configuration, "Local"));
            services.AddSingleton<IJsonSerializer, StandardJsonSerializer>();
            services.AddSingleton<IContentPathProvider>(new ContentPathProvider(_Configuration));

            //Queues
            _RegionSyncEnabled = _Configuration.GetValue("RegionSync", true);
            if (_RegionSyncEnabled)
            {
                services.AddScoped<IQueueSender<StorageAccountSyncMessage>, QueueSendCommand<StorageAccountSyncMessage>>();
                services.AddSingleton<IServiceBusConfig>(new ServiceBusConfig(_Configuration, "ServiceBus"));
            }
            else
            {
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var certificateHack = _Configuration.GetValue("CertificateHack", false);
            if (certificateHack)
            {
                logger.LogWarning("Unproven hack for self-signed certificates is enabled.");
                ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) =>
                    true;
            }

            if (_RegionSyncEnabled)
            {
                logger.LogInformation($"Writing to queue for sync across regions is enabled: true.");
            }
            else
            {
                logger.LogWarning($"Writing to queue for sync across regions is enabled: false.");
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
