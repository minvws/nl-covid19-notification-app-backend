// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.MvcHooks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services;
using Microsoft.Extensions.Logging;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.WorkflowApi
{
    public class Startup
    {
        private bool _SignatureValidationEnabled;
        private const string Title = "MSS Workflow Api";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ComponentsContainerHelper.RegisterDefaultServices(services);

            services.AddControllers();
            services.AddSeriLog(Configuration);

            //services.AddMvc(options => options.Filters.Add(new SerilogServiceExceptionInterceptor(services)));

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "WorkFlow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IGeanTekListValidationConfig, StandardGeanCommonWorkflowConfig>();
            services.AddSingleton<ITemporaryExposureKeyValidator, TemporaryExposureKeyValidator>();
            services.AddSingleton<ITemporaryExposureKeyValidatorConfig, TemporaryExposureKeyValidatorConfig>();

            services.AddScoped<HttpPostReleaseTeksCommand, HttpPostReleaseTeksCommand>();

            services.AddScoped<PollTokenGenerator, PollTokenGenerator>();

            services.AddScoped<IReleaseTeksValidator, ReleaseTeksValidator>();
            services.AddScoped<ISignatureValidator, SignatureValidator>();

            _SignatureValidationEnabled = Configuration.GetValue("ValidatePostKeysSignature", true);
            if (_SignatureValidationEnabled)
            {
                services.AddScoped<ISignatureValidator, SignatureValidator>();
            }
            else
            {
                services.AddScoped<ISignatureValidator, DoNotValidateSignatureValidator>();
            }

            services.AddScoped<HttpPostRegisterSecret, HttpPostRegisterSecret>();
            services.AddScoped<ISecretWriter, SecretWriter>();
            services.AddScoped<ISecretConfig, StandardSecretConfig>();
            services.AddScoped<ITekWriter, TekWriter>();
            services.AddScoped<RandomNumberGenerator, RandomNumberGenerator>();

            // CaregiverPortal scopes

            services.AddScoped<JwtService, JwtService>();
            services.AddScoped<PollTokenGenerator, PollTokenGenerator>();
            services.AddAuthentication("icc_jwt")
                .AddScheme<AuthenticationSchemeOptions, JwtAuthorizationHandler>("icc_jwt", null);

            
            
            services.AddScoped<HttpPostAuthorise, HttpPostAuthorise>();
            services.AddScoped<HttpPostLabVerify, HttpPostLabVerify>();
            services.AddScoped<IAuthorisationWriter, AuthorisationWriter>();
            services.AddScoped<LabVerifyChecker, LabVerifyChecker>();
            
            services.AddSwaggerGen(o => { o.SwaggerDoc("v1", new OpenApiInfo {Title = Title, Version = "v1"}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            app.UseDeveloperExceptionPage();

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
            app.UseSwaggerUI(o => { o.SwaggerEndpoint("/swagger/v1/swagger.json", Title); });

            if (!env.IsDevelopment())
                app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}