// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

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

            serviceProvider.GetRequiredService<GenerateTeksCommand>().Execute(args2).GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton(x => DbContextStartup.Workflow(x, false));

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddSingleton<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();

            services.AddTransient(x => new GenerateTeksCommand(
                x.GetRequiredService<IRandomNumberGenerator>(),
                x.GetRequiredService<WorkflowDbContext>(), 
                x.GetRequiredService<TekReleaseWorkflowStateCreate>
                ));

            services.AddTransient<TekReleaseWorkflowStateCreate>();
            
            services.AddTransient<IWorkflowTime, TekReleaseWorkflowTime>();
        }
    }
}
