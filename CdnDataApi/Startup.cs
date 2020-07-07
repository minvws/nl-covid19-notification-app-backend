// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Authentication;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.CdnDataApi
{
    public class Startup
    {
        private const string Title = "MSS CDN Data API";
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public IHostingEnvironment CurrentEnvironment { get; set; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ComponentsContainerHelper.RegisterDefaultServices(services);

            services.AddSeriLog(Configuration);

            services.AddControllers().AddJsonOptions(_ =>
            {
                // This configures the serializer for ASP.Net, StandardContentEntityFormatter does that for ad-hoc occurrences.
                _.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            services.AddBasicAuthentication();

            services.AddScoped<HttpGetManifestBinaryContentCommand, HttpGetManifestBinaryContentCommand>();
            services.AddScoped<DynamicManifestReader, DynamicManifestReader>();
            services.AddScoped<ManifestBuilder, ManifestBuilder>();
            services.AddSingleton<IUtcDateTimeProvider>(new StandardUtcDateTimeProvider());
            services.AddSingleton<IPublishingId>(new StandardPublishingIdFormatter());
            services.AddSingleton<IGaenContentConfig>(new GaenContentConfig(Configuration));

            if (CurrentEnvironment.IsProduction() || CurrentEnvironment.IsStaging())
            {
                services.AddSingleton<IContentSigner, CmsSigner>();
                services.AddSingleton<ICertificateProvider, HsmCertificateProvider>();
                services.AddSingleton<IThumbprintConfig>(x => new CertificateProviderConfig(x.GetService<IConfiguration>(), "ExposureKeySets:Signing:NL"));
            }
            else
            {
                services.AddSingleton<IContentSigner, FakeContentSigner>();
            }

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ExposureContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddScoped<GetActiveExposureKeySetsListCommand, GetActiveExposureKeySetsListCommand>();
            services.AddScoped<GetLatestContentCommand<AppConfigContentEntity>, GetLatestContentCommand<AppConfigContentEntity>>();
            services.AddScoped<GetLatestContentCommand<ResourceBundleContentEntity>, GetLatestContentCommand<ResourceBundleContentEntity>>();
            services.AddScoped<GetLatestContentCommand<RiskCalculationContentEntity>, GetLatestContentCommand<RiskCalculationContentEntity>>();

            services.AddScoped<HttpGetBinaryContentCommand<ExposureKeySetContentEntity>, HttpGetBinaryContentCommand<ExposureKeySetContentEntity>>();
            services.AddScoped<HttpGetBinaryContentCommand<AppConfigContentEntity>, HttpGetBinaryContentCommand<AppConfigContentEntity>>();
            services.AddScoped<HttpGetBinaryContentCommand<ResourceBundleContentEntity>,  HttpGetBinaryContentCommand<ResourceBundleContentEntity>>();
            services.AddScoped<HttpGetBinaryContentCommand<RiskCalculationContentEntity>, HttpGetBinaryContentCommand<RiskCalculationContentEntity>>();

            services.AddScoped<IReader<ExposureKeySetContentEntity >, SafeBinaryContentDbReader<ExposureKeySetContentEntity>>();
            services.AddScoped<IReader<AppConfigContentEntity>, SafeBinaryContentDbReader<AppConfigContentEntity>>();
            services.AddScoped<IReader<ResourceBundleContentEntity>, SafeBinaryContentDbReader<ResourceBundleContentEntity>>();
            services.AddScoped<IReader<RiskCalculationContentEntity>, SafeBinaryContentDbReader<RiskCalculationContentEntity>>();


            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = Title,
                    Version = "v1",
                });

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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.ConfigObject.ShowExtensions = true;
                o.SwaggerEndpoint("../swagger/v1/swagger.json", Title);
            });
            app.UseHttpsRedirection();
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
