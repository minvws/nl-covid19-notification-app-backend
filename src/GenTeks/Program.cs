// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestDataGeneration.Commands;

namespace GenTeks
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
            return 0;
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var args2 = new GenerateTeksCommandArgs
            {
                WorkflowCount = args.Length > 0 && int.TryParse(args[0], out var v0) ? v0 : 10,
                TekCountPerWorkflow = args.Length > 1 && int.TryParse(args[1], out var v1) ? v1 : 14,
            };

            serviceProvider.GetRequiredService<GenerateTeksCommand>().ExecuteAsync(args2).GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient(x => x.CreateDbContext(y => new WorkflowDbContext(y), DatabaseConnectionStringNames.Workflow));

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddSingleton<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();

            services.AddTransient(x => new GenerateTeksCommand(
                x.GetRequiredService<IRandomNumberGenerator>(),
                x.GetRequiredService<WorkflowDbContext>, 
                x.GetRequiredService<TekReleaseWorkflowStateCreate>
                ));

            services.AddTransient<TekReleaseWorkflowStateCreate>();
            services.AddTransient<IWorkflowTime, TekReleaseWorkflowTime>();

            services.AddSingleton<RegisterSecretLoggingExtensions>();
        }
    }
}
