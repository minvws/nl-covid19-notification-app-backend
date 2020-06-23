// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Authentication;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EKSEngineApi
{
    public class Startup
    {
        private const string Title = "MSS EKSEngine Api";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options => 
            { 
                options.RespectBrowserAcceptHeader = true; 
            }).AddNewtonsoftJson(options =>
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver());

            services.AddSeriLog(Configuration);
            services.AddBasicAuthentication();

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "WorkFlow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                return result;
            });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ExposureContentDbContext(builder.Build());
                return result;
            });

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IGaenContentConfig, GaenContentConfig>();
            services.AddScoped<IExposureKeySetHeaderInfoConfig, ExposureKeySetHeaderInfoConfig>();
            services.AddScoped<IContentSigner, FakeContentSigner>();

            services.AddSingleton(x =>
            {
                var config = new CertificateProviderConfig(Configuration, "Content");
                var provider = new HsmCertificateProvider(config.Thumbprint);
                var signer = new CmsSigner(provider);
                return signer;
            });

            services.AddSingleton(x =>
            {
                var config = new CertificateProviderConfig(Configuration, "KeySet");
                var provider = new HsmCertificateProvider(config.Thumbprint);
                var signer = new EcdSaSigner(provider);
                return signer;
            });

            services.AddScoped<HttpPostGenerateExposureKeySetsCommand, HttpPostGenerateExposureKeySetsCommand>();
            

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo { Title = Title, Version = "v1" });

                o.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header using the Bearer scheme."
                });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basic"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("../swagger/v1/swagger.json", Title);
            });

            if (!env.IsDevelopment()) 
                app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes
            
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
