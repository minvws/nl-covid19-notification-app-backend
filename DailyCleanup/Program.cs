// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ServiceRegHelpers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

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

            var j0 = serviceProvider.GetRequiredService<SnapshotWorkflowTeksToDksCommand>();
            var j1 = serviceProvider.GetRequiredService<ExposureKeySetBatchJobMk3>();
            //Remove this one when we get manifest count down to 1.
            var j2 = serviceProvider.GetRequiredService<ManifestUpdateCommand>();
            var j3 = serviceProvider.GetRequiredService<RemoveExpiredManifestsCommand>();
            var j4 = serviceProvider.GetRequiredService<RemoveExpiredEksCommand>();
            var j5 = serviceProvider.GetRequiredService<RemoveExpiredWorkflowsCommand>();
            var j6 = serviceProvider.GetRequiredService<IStatisticsCommand>();
            //Remove this one when we have 1 cert chain.
            var j7 = serviceProvider.GetRequiredService<NlContentResignExistingV1ContentCommand>();
            //Clean these two up once the V1 endpoint is deprecated
            var j8 = serviceProvider.GetRequiredService<RemoveExpiredEksV2Command>();
            var j9 = serviceProvider.GetRequiredService<RemoveExpiredManifestsV2Command>();
            var j10 = serviceProvider.GetRequiredService<RemovePublishedDiagnosticKeys>();

            logger.WriteSnaphotWorkflowTeksToDks();
            j0.ExecuteAsync().GetAwaiter().GetResult();

            logger.WriteEksEngineStarting();
            j1.ExecuteAsync().GetAwaiter().GetResult();
            
            logger.WriteManifestEngineStarting();
            j2.ExecuteAsync().GetAwaiter().GetResult();
            j2.ExecuteForV3().GetAwaiter().GetResult();
            
            logger.WriteDailyStatsCalcStarting();
            j6.Execute();
            
            logger.WriteManiFestCleanupStarting();
            j3.ExecuteAsync().GetAwaiter().GetResult();
            
            logger.WriteEksCleanupStarting();
            j4.Execute();
            
            logger.WriteWorkflowCleanupStarting();
            j5.Execute();
            
            logger.WriteResignerStarting();
            j7.ExecuteAsync().GetAwaiter().GetResult();
            
            logger.WriteEksV2CleanupStarting();
            j8.Execute();
            
            logger.WriteManifestV2CleanupStarting();
            j9.ExecuteAsync().GetAwaiter().GetResult();

            j10.Execute();

            logger.WriteFinished();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient(x => DbContextStartup.Workflow(x, false));
            services.AddTransient(x => DbContextStartup.Content(x, false));
            services.AddTransient(x => DbContextStartup.Publishing(x, false));
            services.AddTransient(x => DbContextStartup.Stats(x, false));
            services.AddTransient(x => DbContextStartup.DkSource(x, false));

            services.AddTransient<Func<WorkflowDbContext>>(x => x.GetService<WorkflowDbContext>);
            services.AddTransient<Func<EksPublishingJobDbContext>>(x => x.GetService<EksPublishingJobDbContext>);
            services.AddTransient<Func<ContentDbContext>>(x => x.GetService<ContentDbContext>);
            services.AddTransient<Func<DkSourceDbContext>>(x => x.GetService<DkSourceDbContext>);

            services.AddSingleton<IWrappedEfExtensions, SqlServerWrappedEfExtensions>();

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.EksEngine();

            services.ManifestEngine();

            services.AddTransient<RemoveExpiredManifestsCommand>();
            services.AddTransient<RemoveExpiredManifestsV2Command>();
            services.AddTransient<RemoveExpiredEksCommand>();
            services.AddTransient<RemoveExpiredEksV2Command>();
            services.AddTransient<RemoveExpiredWorkflowsCommand>();
            services.AddTransient<RemovePublishedDiagnosticKeys>();
            
            services.AddSingleton<IManifestConfig, ManifestConfig>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();

            services.AddSingleton<EksBuilderV1LoggingExtensions>();
            services.AddSingleton<DailyCleanupLoggingExtensions>();
            services.AddSingleton<ExpiredManifestLoggingExtensions>();
            services.AddSingleton<ExpiredEksLoggingExtensions>();
            services.AddSingleton<ExpiredWorkflowLoggingExtensions>();
            services.AddSingleton<ResignerLoggingExtensions>();
            services.AddSingleton<ExpiredEksV2LoggingExtensions>();
            services.AddSingleton<ExpiredManifestV2LoggingExtensions>();
            services.AddSingleton<EksEngineLoggingExtensions>();
            services.AddSingleton<SnapshotLoggingExtensions>();
            services.AddSingleton<EksJobContentWriterLoggingExtensions>();
            services.AddSingleton<MarkWorkFlowTeksAsUsedLoggingExtensions>();
            services.AddSingleton<ManifestUpdateCommandLoggingExtensions>();
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();


            services.NlResignerStartup();

            services.DummySignerStartup();
            services.GaSignerStartup();

            services.DailyStatsStartup();

            services.ManifestForV3Startup();
        }
    }
}
