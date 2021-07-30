// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;

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
            var logger = serviceProvider.GetRequiredService<DailyCleanupLoggingExtensions>();
            logger.WriteStart();

            var run = new List<Action>();

            var c90 = serviceProvider.GetRequiredService<IStatisticsCommand>();
            run.Add(() => logger.WriteDailyStatsCalcStarting());
            run.Add(() => c90.Execute());

            var c80 = serviceProvider.GetRequiredService<RemoveExpiredWorkflowsCommand>();
            run.Add(() => logger.WriteWorkflowCleanupStarting());
            run.Add(() => c80.ExecuteAsync().GetAwaiter().GetResult());

            var c126 = serviceProvider.GetRequiredService<RemoveDiagnosisKeysReadyForCleanup>();
            run.Add(() => c126.ExecuteAsync().GetAwaiter().GetResult());

            var c125 = serviceProvider.GetRequiredService<RemovePublishedDiagnosisKeys>();
            run.Add(() => c125.ExecuteAsync().GetAwaiter().GetResult());

            logger.WriteEksCleanupStarting();
            var c70 = serviceProvider.GetRequiredService<RemoveExpiredEksCommand>();
            run.Add(() => logger.WriteEksCleanupStarting());
            run.Add(() => c70.ExecuteAsync().GetAwaiter().GetResult());

            var c110 = serviceProvider.GetRequiredService<RemoveExpiredEksV2Command>();
            run.Add(() => logger.WriteEksV2CleanupStarting());
            run.Add(() => c110.ExecuteAsync().GetAwaiter().GetResult());

            var c60 = serviceProvider.GetRequiredService<RemoveExpiredManifestsCommand>();
            run.Add(() => logger.WriteManiFestCleanupStarting());
            run.Add(() => c60.Execute());

            var c140 = serviceProvider.GetRequiredService<RemoveExpiredIksInCommand>();
            run.Add(() => c140.ExecuteAsync().GetAwaiter().GetResult());

            var c150 = serviceProvider.GetRequiredService<RemoveExpiredIksOutCommand>();
            run.Add(() => c150.ExecuteAsync().GetAwaiter().GetResult());

            run.Add(() => logger.WriteFinished());

            foreach (var i in run)
            {
                i();
            }
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<WorkflowDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.Workflow)));
            services.AddDbContext<DkSourceDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.DkSource)));
            services.AddDbContext<ContentDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.Content)));
            services.AddDbContext<IksInDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksIn)));
            services.AddDbContext<IksOutDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksOut)));
            services.AddDbContext<IksPublishingJobDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksPublishing)));
            services.AddDbContext<EksPublishingJobDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.EksPublishing)));
            services.AddDbContext<StatsDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.Stats)));

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IIksCleaningConfig, IksCleaningConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();
            services.AddSingleton<IManifestConfig, ManifestConfig>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();
            services.AddSingleton<IAcceptableCountriesSetting, EfgsInteropConfig>();
            services.AddSingleton<IOutboundFixedCountriesOfInterestSetting, EfgsInteropConfig>();

            services.AddTransient<RemoveExpiredIksInCommand>();
            services.AddTransient<RemoveExpiredIksOutCommand>();
            services.AddTransient<RemoveExpiredWorkflowsCommand>();
            services.AddTransient<RemoveDuplicateDiagnosisKeysCommand>();
            services.AddTransient<RemovePublishedDiagnosisKeys>();
            services.AddTransient<RemoveDiagnosisKeysReadyForCleanup>();
            services.AddTransient<RemoveExpiredEksCommand>();
            services.AddTransient<RemoveExpiredEksV2Command>();
            services.AddTransient<RemoveExpiredManifestsReceiver>();
            services.AddTransient<RemoveExpiredManifestsCommand>();

            services.AddSingleton<DailyCleanupLoggingExtensions>();
            services.AddSingleton<ExpiredEksLoggingExtensions>();
            services.AddSingleton<ExpiredWorkflowLoggingExtensions>();
            services.AddSingleton<ExpiredEksV2LoggingExtensions>();
            services.AddSingleton<RemoveExpiredIksLoggingExtensions>();
            
            services.DailyStatsStartup();
        }
    }
}
