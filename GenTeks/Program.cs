// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;

namespace GenTeks
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var args2 = new GenerateTeksCommandArgs
            {
                WorkflowCount = args.Length > 0 && int.TryParse(args[0], out var v0) ? v0 : 10,
                TekCountPerWorkflow = args.Length > 1 && int.TryParse(args[1], out var v1) ? v1 : 14,
                Authorised = args.Length > 2 && bool.TryParse(args[2], out var v2) ? v2 : true, //NB Resharper nag wrong
                Seed = args.Length > 3 && int.TryParse(args[3], out var v3) ? v3 : 2345,
            };

            serviceProvider.GetRequiredService<GenerateTeksCommand>().Execute(args2);
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped(x => DbContextStartup.Workflow(x));
            services.AddTransient<GenerateTeksCommand>();
        }
    }
}
