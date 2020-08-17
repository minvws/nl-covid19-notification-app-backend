// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    public class Startup
    {
        private bool _SignatureValidationEnabled;
        private const string Title = "Mobile App API";

        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private readonly IConfiguration _Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IJsonSerializer, StandardJsonSerializer>();

            services.AddControllers();

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped(x => DbContextStartup.Workflow(x));

            services.AddScoped<HttpPostReleaseTeksCommand2>();
            services.AddScoped<HttpPostRegisterSecret>();
            services.AddScoped<HttpPostDecoyKeysCommand>();

            services.AddSingleton<ITekListValidationConfig, StandardTekListValidationConfig>();
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IDecoyKeysConfig, StandardDecoyKeysConfig>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();

            if (_Configuration.ValidatePostKeysSignature())
                services.AddTransient<ISignatureValidator, SignatureValidator>();
            else
                services.AddTransient<ISignatureValidator, DoNotValidateSignatureValidator>();

            services.AddTransient<IWorkflowTime, TekReleaseWorkflowTime>();
            services.AddTransient<IPostTeksValidator, PostTeksArgsValidator>();
            services.AddTransient<ITemporaryExposureKeyValidator, TemporaryExposureKeyValidator>();
            services.AddTransient<ITekListWorkflowFilter, BackwardCompatibleV15TekListWorkflowFilter>();
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddTransient<ISecretWriter, TekReleaseWorkflowStateCreate>();
            services.AddTransient<ITekWriter, TekWriter>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddTransient<ILabConfirmationIdFormatter,StandardLabConfirmationIdFormatter>();

            services.AddScoped<ResponsePaddingFilterAttribute>();
            services.AddScoped<IResponsePaddingConfig, ResponsePaddingConfig>();
            services.AddScoped<IPaddingGenerator, CryptoRandomPaddingGenerator>();
            
            services.AddSwaggerGen(o => { o.SwaggerDoc("v1", new OpenApiInfo {Title = Title, Version = "v1"}); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var logger = services.GetService<ILogger<Startup>>();
            if (_SignatureValidationEnabled)
            {
                logger.LogInformation("Signature validation of POST postkeys enabled : true");
            }
            else
            {
                logger.LogWarning("Signature validation of POST postkeys enabled : false");
            }


            app.UseSwagger();
            app.UseSwaggerUI(o => { o.SwaggerEndpoint("v1/swagger.json", Title); });

            if (!env.IsDevelopment())
                app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}