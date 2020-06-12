// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.KeysFirstWorkflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks;

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
            services.AddControllers(options => 
            { 
                options.RespectBrowserAcceptHeader = true; 
            });
                
            services.AddControllers();

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<ILuhnModNConfig, LuhnModNConfig>();
            services.AddSingleton<IGaenContentConfig, GaenContentConfig>();
            services.AddSingleton<IExposureKeySetHeaderInfoConfig, HsmExposureKeySetHeaderInfoConfig>();
            services.AddSingleton<IExposureKeySetBatchJobConfig, ExposureKeySetBatchJobConfig>();
            services.AddSingleton<IPublishingId>(x => new Sha256PublishingId(new HardCodedExposureKeySetSigning()));

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
                var config = new StandardEfDbConfig(Configuration, "Job");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                return new ExposureKeySetsBatchJobDbContext(builder.Build());
            });

            //Just for the Batch Job
            services.AddScoped<IEfDbConfig>(x => new StandardEfDbConfig(Configuration, "Job"));

            services.AddScoped<HttpPostKeysFirstGenerateTekSetsCommand, HttpPostKeysFirstGenerateTekSetsCommand>();
            services.AddScoped<HttpPostKeysFirstEscrowCommand, HttpPostKeysFirstEscrowCommand>();

            services.AddSingleton<IKeysFirstEscrowValidator, KeysFirstEscrowValidator>();
            services.AddSingleton<IKeysFirstAuthorisationTokenValidator, KeysFirstAuthorisationTokenLuhnModNValidator>();
            services.AddSingleton<IGeanTekListValidationConfig, StandardGeanCommonWorkflowConfig>();
            services.AddSingleton<ITemporaryExposureKeyValidator, TemporaryExposureKeyValidator>();
            services.AddSingleton<ITemporaryExposureKeyValidatorConfig, TemporaryExposureKeyValidatorConfig>();
            services.AddScoped<IKeysFirstEscrowWriter, KeysFirstEscrowInsertDbCommand>();

            services.AddScoped<HttpPostKeysFirstAuthorisationCommand, HttpPostKeysFirstAuthorisationCommand>();
            services.AddScoped<IKeysFirstAuthorisationWriter, KeysFirstDbAuthoriseCommand>();
            
            //TODO services.AddScoped<HttpGetLatestManifestCommand, HttpGetLatestManifestCommand>();
            services.AddScoped<ManifestBuilder, ManifestBuilder>();
            services.AddScoped<GetActiveExposureKeySetsListCommand, GetActiveExposureKeySetsListCommand>();
            services.AddScoped<GetLatestRiskCalculationParametersCommand, GetLatestRiskCalculationParametersCommand>();
            services.AddScoped<GetLatestResourceBundleCommand, GetLatestResourceBundleCommand>();

            services.AddScoped<ExposureKeySetSafeReadCommand, ExposureKeySetSafeReadCommand>();
            
            services.AddScoped<SafeGetRiskCalculationConfigDbCommand, SafeGetRiskCalculationConfigDbCommand>();

            services.AddScoped<HttpPostRiskCalculationConfigCommand, HttpPostRiskCalculationConfigCommand>();
            services.AddScoped<RiskCalculationConfigValidator, RiskCalculationConfigValidator>();
            services.AddScoped<RiskCalculationConfigInsertDbCommand, RiskCalculationConfigInsertDbCommand>();

            services.AddScoped<HttpPostResourceBundleCommand, HttpPostResourceBundleCommand>();
            services.AddScoped<ResourceBundleInsertDbCommand, ResourceBundleInsertDbCommand>();
            services.AddScoped<ResourceBundleValidator, ResourceBundleValidator>();

            //services.AddScoped<SafeGetResourceBundleCommand, SafeGetResourceBundleCommand>();

            services.AddScoped<ProvisionDatabasesCommand, ProvisionDatabasesCommand>();
            services.AddScoped<GenerateKeysFirstTekSetsArgs, GenerateKeysFirstTekSetsArgs>();
            services.AddScoped<HttpPostGenerateExposureKeySetsCommand, HttpPostGenerateExposureKeySetsCommand>();
            services.AddScoped<HttpPostKeysFirstRandomAuthorisationCommand, HttpPostKeysFirstRandomAuthorisationCommand>();
            services.AddScoped<GenerateKeysFirstAuthorisations, GenerateKeysFirstAuthorisations>();

            services.AddScoped<HttpGetCdnContentCommand<ManifestEntity>, HttpGetCdnContentCommand<ManifestEntity>>();
            services.AddScoped<HttpGetCdnContentCommand<ExposureKeySetContentEntity>, HttpGetCdnContentCommand<ExposureKeySetContentEntity>>();
            services.AddScoped<HttpGetCdnContentCommand<RiskCalculationContentEntity>, HttpGetCdnContentCommand<RiskCalculationContentEntity>>();
            services.AddScoped<HttpGetCdnContentCommand<ResourceBundleContentEntity>, HttpGetCdnContentCommand<ResourceBundleContentEntity>>();

            services.AddScoped<IReader<ManifestEntity>, DynamicManifestReader>();
            services.AddScoped<IReader<ExposureKeySetContentEntity>, SafeBinaryContentDbReader<ExposureKeySetContentEntity>>();
            services.AddScoped<IReader<ResourceBundleContentEntity>, SafeBinaryContentDbReader<ResourceBundleContentEntity>>();
            services.AddScoped<IReader<RiskCalculationContentEntity>, SafeBinaryContentDbReader<RiskCalculationContentEntity>>();

            services.AddScoped<HttpPostKeysLastRegisterSecret, HttpPostKeysLastRegisterSecret>();
            services.AddScoped<IKeysLastSecretValidator, KeysLastSecretValidator>();
            services.AddScoped<IKeysLastSecretConfig, StandardKeysLastSecretConfig>();

            services.AddScoped<IKeysLastSecretWriter, KeysLastSecretWriter>();
            
            services.AddScoped<KeysLastAuthorisationWriter, KeysLastAuthorisationWriter>();

            services.AddScoped<HttpPostKeysLastReleaseTeksCommand, HttpPostKeysLastReleaseTeksCommand>();
            services.AddScoped<IKeysLastReleaseTeksValidator, KeysLastReleaseTeksValidator>();
            services.AddScoped<IKeysLastAuthorisationTokenValidator, FakeKeysLastReleaseTeksValidator>();

            services.AddScoped<IKeysLastTekWriter, FakeKeysLastTekWriter>();

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "Dutch Exposure Notification API (inc. dev support)", 
                    Version = "v1", 
                    Description = "This specification describes the interface between the Dutch exposure notification app and the backend service.\nTODO: Add signatures to manifest, riskcalculationparameters and appconfig",
                    Contact = new OpenApiContact {
                        Name = "Ministerie van Volksgezondheid Welzijn en Sport backend repository", //TODO looks wrong?
                        Url = new Uri("https://github.com/minvws/nl-covid19-notification-app-backend"),
                    },
                    License = new OpenApiLicense 
                    { 
                        Name = "European Union Public License v. 1.2",
                        //TODO this should be https://joinup.ec.europa.eu/collection/eupl/eupl-text-eupl-12
                        Url = new Uri("https://github.com/minvws/nl-covid19-notification-app-backend/blob/master/LICENSE.txt")
                    },
                    
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Dutch Exposure Notification API (inc. dev support)");
            });
            if(!env.IsDevelopment()) app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
