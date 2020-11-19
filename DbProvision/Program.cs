// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace DbProvision
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
            return 0;
        }

        private static void Start(IServiceProvider services, string[] args)
        {
            services.GetRequiredService<ProvisionDatabasesCommand>().ExecuteAsync(args).GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped(x => DbContextStartup.Workflow(x, false));
            services.AddScoped(x => DbContextStartup.Content(x, false));
            services.AddScoped(x => DbContextStartup.Publishing(x, false));

            services.AddTransient<WorkflowDatabaseCreateCommand>();
            services.AddTransient<PublishingJobDatabaseCreateCommand>();
            services.AddTransient<ContentDatabaseCreateCommand>();

            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<ProvisionDatabasesCommand>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<ContentValidator>();
            services.AddTransient<ContentInsertDbCommand>();
            services.AddTransient<ZippedSignedContentFormatter>();

            services.AddSingleton<DbProvisionLoggingExtensions>();

            services.DummySignerStartup();
        }
    }
}
