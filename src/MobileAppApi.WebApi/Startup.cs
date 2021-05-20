// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    public class Startup
    {
        private const string Title = "Mobile App API";

        private readonly bool _IsDev;

        public Startup(IWebHostEnvironment env)
        {
            _IsDev = env?.IsDevelopment() ?? throw new ArgumentException(nameof(env));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IJsonSerializer, StandardJsonSerializer>();

            services.AddControllers();

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped(x => x.CreateDbContext(y => new WorkflowDbContext(y), DatabaseConnectionStringNames.Workflow));

            services.AddScoped<HttpPostReleaseTeksCommand2>();
            services.AddScoped<HttpPostRegisterSecret>();
            services.AddScoped<HttpPostRegisterSecretV2>();

            services.AddSingleton<ITekListValidationConfig, StandardTekListValidationConfig>();
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();
            services.AddTransient<ISignatureValidator, SignatureValidator>();
            services.AddTransient<IWorkflowTime, TekReleaseWorkflowTime>();

            services.AddTransient<ILuhnModNConfig, LuhnModNConfig>();
            services.AddTransient<ILuhnModNGenerator, LuhnModNGenerator>();

            services.AddTransient<IPostTeksValidator, PostTeksArgsValidator>();
            services.AddTransient<ITemporaryExposureKeyValidator, TemporaryExposureKeyValidator>();
            services.AddTransient<ITekListWorkflowFilter, BackwardCompatibleV15TekListWorkflowFilter>();
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddTransient<ISecretWriter, TekReleaseWorkflowStateCreate>();
            services.AddTransient<TekReleaseWorkflowStateCreateV2>();
            services.AddTransient<ITekWriter, TekWriter>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddTransient<ILabConfirmationIdFormatter, StandardLabConfirmationIdFormatter>();
            services.AddTransient<ITekValidPeriodFilter, TekValidPeriodFilter>();
            services.AddScoped<ResponsePaddingFilterAttribute>();
            services.AddScoped<IResponsePaddingConfig, StandardResponsePaddingConfig>();
            services.AddScoped<IPaddingGenerator, CryptoRandomPaddingGenerator>();
            services.AddScoped<SuppressErrorAttribute>();
            services.AddScoped<DecoyTimeGeneratorAttribute>();
            services.AddScoped<DecoyTimeAggregatorAttribute>();
            services.AddSingleton<IDecoyTimeCalculator, DecoyTimeCalculator>();
            services.AddSingleton<IWelfordsAlgorithm, WelfordsAlgorithm>();

            services.AddSingleton<RegisterSecretLoggingExtensions>();
            services.AddSingleton<RegisterSecretLoggingExtensionsV2>();
            services.AddSingleton<PostKeysLoggingExtensions>();
            services.AddSingleton<DecoyKeysLoggingExtensions>();
            services.AddSingleton<ResponsePaddingLoggingExtensions>();
            services.AddSingleton<SuppressErrorLoggingExtensions>();

            if (_IsDev)
                services.AddSwaggerGen(o => { o.SwaggerDoc("v1", new OpenApiInfo { Title = Title, Version = "v1" }); });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_IsDev)
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(o => { o.SwaggerEndpoint("v1/swagger.json", Title); });
            }

            // HttpsRedirection will be handled at the infrastructure level
            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}