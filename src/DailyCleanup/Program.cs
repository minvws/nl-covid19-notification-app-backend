// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
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

            var commandInvoker = new CommandInvoker();
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<StatisticsCommand>());
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<RemoveExpiredWorkflowsCommand>());
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<RemoveDiagnosisKeysReadyForCleanupCommand>());
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<RemovePublishedDiagnosisKeysCommand>());
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<RemoveExpiredEksCommand>());
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<RemoveExpiredEksV2Command>());
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<RemoveExpiredManifestsCommand>());
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<RemoveExpiredIksInCommand>());
            commandInvoker.SetCommand(serviceProvider.GetRequiredService<RemoveExpiredIksOutCommand>());
            commandInvoker.RunAsync().GetAwaiter().GetResult();

            logger.WriteFinished();

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

            services.AddTransient<RemoveExpiredWorkflowsCommand>();
            services.AddTransient<RemoveDiagnosisKeysReadyForCleanupCommand>();
            services.AddTransient<RemovePublishedDiagnosisKeysCommand>();
            services.AddTransient<RemoveExpiredEksCommand>();
            services.AddTransient<RemoveExpiredEksV2Command>();
            services.AddTransient<RemoveExpiredManifestsCommand>();
            services.AddTransient<RemoveExpiredIksInCommand>();
            services.AddTransient<RemoveExpiredIksOutCommand>();

            services.AddTransient<RemoveExpiredManifestsReceiver>();

            services.AddSingleton<DailyCleanupLoggingExtensions>();
            services.AddSingleton<ExpiredEksLoggingExtensions>();
            services.AddSingleton<ExpiredWorkflowLoggingExtensions>();
            services.AddSingleton<ExpiredEksV2LoggingExtensions>();
            services.AddSingleton<RemoveExpiredIksLoggingExtensions>();

            services.DailyStatsStartup();
        }
    }
}
