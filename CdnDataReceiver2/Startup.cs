using System.Net;
using CdnDataReceiver.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace CdnDataReceiver2
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _Configuration = configuration;
            _Environment = environment;
        }

        private readonly IConfiguration _Configuration;
        private readonly IWebHostEnvironment _Environment;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //TODO ensure this is removed once BD finally get the certs.
            var certificateHack = (bool)_Configuration.GetValue(typeof(bool), "CertificateHack", false);
            if (certificateHack)
                ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) =>
                    true;

            services.AddControllers();
            services.AddScoped<HttpPostContentReciever2>();
            
            services.AddScoped<IBlobWriter, StandardBlobWriter>();
            services.AddScoped<ManifestBlobWriter>();
            services.AddSingleton<IStorageAccountConfig>(new StorageAccountAppSettings(_Configuration, "Local"));

            services.AddSingleton<IJsonSerializer, StandardJsonSerializer>();
            services.AddSingleton<IContentPathProvider>(new ContentPathProvider(_Configuration));

            //Queues
            var regionSync = (bool)_Configuration.GetValue(typeof(bool), "RegionSync", true);
            if ((_Environment.IsDevelopment() || _Environment.IsStaging()) && !regionSync) //NB Staging == Acc
            {
                services.AddScoped<IQueueSender<StorageAccountSyncMessage>, NotAQueueSender<StorageAccountSyncMessage>>();
            }
            else
            {
                //Test and Prod
                services.AddScoped<IQueueSender<StorageAccountSyncMessage>, QueueSendCommand<StorageAccountSyncMessage>>();
                services.AddSingleton<IServiceBusConfig>(new ServiceBusConfig(_Configuration, "ServiceBus"));
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
