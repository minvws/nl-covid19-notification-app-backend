// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.BackgroundJobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers;
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone
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
            services
                .AddControllers(options => { options.RespectBrowserAcceptHeader = true; })
                .AddJsonOptions(_ =>
                {
                    // This configures the serializer for ASP.Net, StandardContentEntityFormatter does that for ad-hoc occurrences.
                    _.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            ComponentsContainerHelper.RegisterDefaultServices(services);

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IGaenContentConfig, GaenContentConfig>();
            services.AddSingleton<IExposureKeySetBatchJobConfig, ExposureKeySetBatchJobConfig>();
            services.AddScoped<IPublishingId, StandardPublishingIdFormatter>();

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ExposureContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "WorkFlow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });
            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Icc");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new IccBackendContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            //Just for the Batch Job
            //services.AddScoped<IEfDbConfig>(x => new StandardEfDbConfig(Configuration, "Job"));
            //services.AddScoped<IExposureKeySetHeaderInfoConfig, ExposureKeySetHeaderInfoConfig>();
            services.AddScoped<IContentSigner, FakeContentSigner>();
            services.AddSingleton<IGeanTekListValidationConfig, StandardGeanCommonWorkflowConfig>();
            services.AddSingleton<ITemporaryExposureKeyValidator, TemporaryExposureKeyValidator>();
            services.AddSingleton<ITemporaryExposureKeyValidatorConfig, TemporaryExposureKeyValidatorConfig>();
            services.AddScoped<IPublishingId, StandardPublishingIdFormatter>();


            services.AddScoped(x =>
                new ExposureKeySetBatchJobMk2(
                    x.GetService<IGaenContentConfig>(),
                    x.GetService<IExposureKeySetBuilder>(),
                    x.GetService<WorkflowDbContext>(),
                    x.GetService<ExposureContentDbContext>(),
                    x.GetService<IUtcDateTimeProvider>(),
                    x.GetService<IPublishingId>()
                ));

            services.AddSingleton<IGaenContentConfig, GaenContentConfig>();
            services.AddScoped<IExposureKeySetBuilder>(x =>
                new ExposureKeySetBuilderV1(
                    x.GetService<IExposureKeySetHeaderInfoConfig>(),
                    new EcdSaSigner(new ResourceCertificateProvider("FakeECDSA.p12")),
                    new CmsSigner(new ResourceCertificateProvider("FakeRSA.p12")),
                    x.GetService<IUtcDateTimeProvider>(), //TODO pass in time thru execute
                    new GeneratedProtobufContentFormatter()
                ));

            services.AddScoped<IExposureKeySetHeaderInfoConfig, ExposureKeySetHeaderInfoConfig>();
            services.AddSingleton<ISignatureValidator>(new FakeSignatureValidator());

            services.AddScoped(x =>
                new ExposureKeySetBuilderV1(
                    x.GetService<ExposureKeySetHeaderInfoConfig>(),
                    new FakeContentSigner(),
                    new FakeContentSigner(),
                    x.GetService<IUtcDateTimeProvider>(),
                    new GeneratedProtobufContentFormatter()
                ));

            services.AddScoped<ManifestBuilder, ManifestBuilder>();
            services.AddScoped<GetActiveExposureKeySetsListCommand, GetActiveExposureKeySetsListCommand>();

            services.AddScoped<ExposureKeySetSafeReadCommand, ExposureKeySetSafeReadCommand>();
            services.AddScoped<SafeGetRiskCalculationConfigDbCommand, SafeGetRiskCalculationConfigDbCommand>();

            services.AddScoped<HttpPostRiskCalculationConfigCommand, HttpPostRiskCalculationConfigCommand>();
            services.AddScoped<RiskCalculationConfigValidator, RiskCalculationConfigValidator>();
            services.AddScoped<RiskCalculationConfigInsertDbCommand, RiskCalculationConfigInsertDbCommand>();

            services.AddScoped<HttpPostResourceBundleCommand, HttpPostResourceBundleCommand>();
            services.AddScoped<ResourceBundleInsertDbCommand, ResourceBundleInsertDbCommand>();
            services.AddScoped<ResourceBundleValidator, ResourceBundleValidator>();

            services.AddScoped<ProvisionDatabasesCommand, ProvisionDatabasesCommand>();
            services.AddScoped<ProvisionDatabasesCommandIcc, ProvisionDatabasesCommandIcc>();
            services.AddScoped<HttpPostGenerateExposureKeySetsCommand, HttpPostGenerateExposureKeySetsCommand>();
            //services.AddScoped<HttpGetCdnContentCommand<ManifestEntity>, HttpGetCdnContentCommand<ManifestEntity>>();

            services
                .AddScoped<HttpGetSignedCdnContentOnlyCommand<ExposureKeySetContentEntity>,
                    HttpGetSignedCdnContentOnlyCommand<ExposureKeySetContentEntity>>();
            services
                .AddScoped<HttpGetCdnContentCommand<RiskCalculationContentEntity>,
                    HttpGetCdnContentCommand<RiskCalculationContentEntity>>();
            services
                .AddScoped<HttpGetCdnContentCommand<ResourceBundleContentEntity>,
                    HttpGetCdnContentCommand<ResourceBundleContentEntity>>();
            services
                .AddScoped<HttpGetCdnContentCommand<AppConfigContentEntity>,
                    HttpGetCdnContentCommand<AppConfigContentEntity>>();

            services.AddScoped<DynamicManifestReader, DynamicManifestReader>();
            services
                .AddScoped<IReader<ExposureKeySetContentEntity>, SafeBinaryContentDbReader<ExposureKeySetContentEntity>
                >();
            services
                .AddScoped<IReader<ResourceBundleContentEntity>, SafeBinaryContentDbReader<ResourceBundleContentEntity>
                >();
            services
                .AddScoped<IReader<RiskCalculationContentEntity>,
                    SafeBinaryContentDbReader<RiskCalculationContentEntity>>();
            services.AddScoped<IReader<AppConfigContentEntity>, SafeBinaryContentDbReader<AppConfigContentEntity>>();
            services.AddScoped<PurgeExpiredSecretsDbCommand, PurgeExpiredSecretsDbCommand>();

            services.AddScoped<HttpPostRegisterSecret, HttpPostRegisterSecret>();
            services.AddScoped<RandomNumberGenerator, RandomNumberGenerator>();
            services.AddScoped<ISecretConfig, StandardSecretConfig>();

            services.AddScoped<ISecretWriter, SecretWriter>();

            services.AddScoped<AuthorisationWriter, AuthorisationWriter>();

            services.AddScoped<HttpPostReleaseTeksCommand, HttpPostReleaseTeksCommand>();
            services.AddScoped<IReleaseTeksValidator, ReleaseTeksValidator>();

            services.AddScoped<ITekWriter, TekWriter>();

            services.AddScoped<HttpPostAppConfigCommand, HttpPostAppConfigCommand>();
            services.AddScoped<AppConfigInsertDbCommand, AppConfigInsertDbCommand>();
            services.AddScoped<AppConfigValidator, AppConfigValidator>();

            services.AddScoped<HttpPostAuthorise, HttpPostAuthorise>();
            services.AddScoped<IAuthorisationWriter, AuthorisationWriter>();

            services.AddScoped<IBasicAuthenticationConfig, BasicAuthenticationConfig>();
            services.AddBasicAuthentication();

            services.AddScoped<JwtService, JwtService>();
            services.AddAuthentication("icc_jwt")
                .AddScheme<AuthenticationSchemeOptions, JwtAuthorizationHandler>("icc_jwt", null);

            services
                .AddScoped<GetLatestContentCommand<ResourceBundleContentEntity>,
                    GetLatestContentCommand<ResourceBundleContentEntity>>();
            services
                .AddScoped<GetLatestContentCommand<RiskCalculationContentEntity>,
                    GetLatestContentCommand<RiskCalculationContentEntity>>();
            services
                .AddScoped<GetLatestContentCommand<AppConfigContentEntity>,
                    GetLatestContentCommand<AppConfigContentEntity>>();

            services.AddScoped<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddScoped<ZippedSignedContentFormatter, ZippedSignedContentFormatter>();

            services.AddScoped<HttpGetManifestBinaryContentCommand, HttpGetManifestBinaryContentCommand>();
            services.AddScoped<DynamicManifestReader, DynamicManifestReader>();
            services.AddScoped<HttpGetManifestSasCommand, HttpGetManifestSasCommand>();

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Dutch Exposure Notification API (inc. dev support)",
                    Version = "v1",
                    Description =
                        "This specification describes the interface between the Dutch exposure notification app and the backend service.\nTODO: Add signatures to manifest, riskcalculationparameters and appconfig",
                    Contact = new OpenApiContact
                    {
                        Name = "Ministerie van Volksgezondheid Welzijn en Sport backend repository", //TODO looks wrong?
                        Url = new Uri("https://github.com/minvws/nl-covid19-notification-app-backend"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "European Union Public License v. 1.2",
                        //TODO this should be https://joinup.ec.europa.eu/collection/eupl/eupl-text-eupl-12
                        Url = new Uri(
                            "https://github.com/minvws/nl-covid19-notification-app-backend/blob/master/LICENSE.txt")
                    },
                });
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                    "NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.xml"));
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                    "NL.Rijksoverheid.ExposureNotification.BackEnd.Components.xml"));


                o.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header."
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
                        new string[] { }
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:4200").WithHeaders("Authorization", "content-type"));

            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.ConfigObject.ShowExtensions = true;
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Dutch Exposure Notification API (inc. dev support)");
            });
            if (!env.IsDevelopment())
                app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes
            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}