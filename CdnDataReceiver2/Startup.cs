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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<HttpPostContentReciever2>();
            services.AddScoped<IBlobWriter, StandardBlobWriter>();
            services.AddSingleton<IStorageAccountConfig>(new StorageAccountAppSettings(Configuration, "Local"));
            services.AddScoped<ManifestBlobWriter>();
            services.AddScoped<IQueueSender<StorageAccountSyncMessage>, QueueSendCommand<StorageAccountSyncMessage>>();
            services.AddSingleton<IServiceBusConfig>(new ServiceBusConfig(Configuration, "ServiceBus"));
            services.AddSingleton<IJsonSerializer, StandardJsonSerializer>();
            services.AddSingleton<IContentPathProvider>(new ContentPathProvider(Configuration));
            
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

            //TODO ensure this is removed once BD finally get the certs.
            ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) => 
                true;

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
