// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.ServiceRegistrations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                new ConsoleAppRunner().Execute(args, Configure, Start);
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            serviceProvider.GetRequiredService<DailyCleanupJob>().Run();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);

            services.DbContextRegistration(configuration);
            services.SettingsRegistration();
            services.DailyCleanupJobRegistration();
            services.DailyStatsRegistration();
        }
    }
}
