// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound.Publishing;
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

            var run = new List<Action>();

            //TODO read EFGS run.

            var c10 = serviceProvider.GetRequiredService<SnapshotWorkflowTeksToDksCommand>();
            run.Add(() => logger.WriteSnaphotWorkflowTeksToDks());
            run.Add(() => c10.ExecuteAsync().GetAwaiter().GetResult());

            var c20 = serviceProvider.GetRequiredService<IksImportBatchJob>();
            run.Add(() => c20.ExecuteAsync().GetAwaiter().GetResult());

            var c30 = serviceProvider.GetRequiredService<ExposureKeySetBatchJobMk3>();
            run.Add(() => logger.WriteEksEngineStarting());
            run.Add(() => c30.ExecuteAsync().GetAwaiter().GetResult());

            var c40 = serviceProvider.GetRequiredService<ManifestUpdateCommand>();
            run.Add(() => logger.WriteManifestEngineStarting());
            run.Add(() => c40.ExecuteAsync().GetAwaiter().GetResult());

            var c50 = serviceProvider.GetRequiredService<NlContentResignExistingV1ContentCommand>();
            //run.Add(() =>  TODO );
            run.Add(() => c50.ExecuteAsync().GetAwaiter().GetResult());

            var c35 = serviceProvider.GetRequiredService<IksEngine>();
            //run.Add(() =>  TODO );
            run.Add(() => c35.ExecuteAsync().GetAwaiter().GetResult());

            //TODO write EFGS run.

            var c60 = serviceProvider.GetRequiredService<RemoveExpiredManifestsCommand>();
            run.Add(() => logger.WriteManiFestCleanupStarting());
            run.Add(() => c60.ExecuteAsync().GetAwaiter().GetResult());

            logger.WriteEksCleanupStarting();
            var c70 = serviceProvider.GetRequiredService<RemoveExpiredEksCommand>();
            run.Add(() => logger.WriteEksCleanupStarting());
            run.Add(() => c70.Execute());

            var c80 = serviceProvider.GetRequiredService<RemoveExpiredWorkflowsCommand>();
            run.Add(() => logger.WriteWorkflowCleanupStarting());
            run.Add(() => c80.Execute());

            var c90 = serviceProvider.GetRequiredService<IStatisticsCommand>();
            run.Add(() => logger.WriteDailyStatsCalcStarting());
            run.Add(() => c90.Execute());

            var c110 = serviceProvider.GetRequiredService<RemoveExpiredEksV2Command>();
            run.Add(() => logger.WriteEksV2CleanupStarting());
            run.Add(() => c110.Execute());

            var c120 = serviceProvider.GetRequiredService<RemoveExpiredManifestsV2Command>();
            run.Add(() => logger.WriteManifestV2CleanupStarting());
            run.Add(() => c120.ExecuteAsync().GetAwaiter().GetResult());

            var c130 = serviceProvider.GetRequiredService<RemovePublishedDiagnosticKeys>();
            //run.Add(() => TODO);
            run.Add(() => c130.Execute());

            var c100 = serviceProvider.GetRequiredService<NlContentResignExistingV1ContentCommand>();
            run.Add(() => logger.WriteResignerStarting());
            run.Add(() => c100.ExecuteAsync().GetAwaiter().GetResult());

            run.Add(() => logger.WriteFinished());

            foreach (var i in run)
                i();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient(x => DbContextStartup.Workflow(x, false));
            services.AddTransient(x => DbContextStartup.Content(x, false));
            services.AddTransient(x => DbContextStartup.EksPublishing(x, false));
            services.AddTransient(x => DbContextStartup.Stats(x, false));
            services.AddTransient(x => DbContextStartup.DkSource(x, false));
            services.AddTransient(x => DbContextStartup.IksIn(x, false));
            services.AddTransient(x => DbContextStartup.IksPublishing(x, false));
            services.AddTransient(x => DbContextStartup.IksOut(x, false));

            services.AddTransient<Func<WorkflowDbContext>>(x => x.GetService<WorkflowDbContext>);
            services.AddTransient<Func<ContentDbContext>>(x => x.GetService<ContentDbContext>);
            services.AddTransient<Func<EksPublishingJobDbContext>>(x => x.GetService<EksPublishingJobDbContext>);
            services.AddTransient<Func<StatsDbContext>>(x => x.GetService<StatsDbContext>);
            services.AddTransient<Func<DkSourceDbContext>>(x => x.GetService<DkSourceDbContext>);
            services.AddTransient<Func<IksInDbContext>>(x => x.GetService<IksInDbContext>);
            services.AddTransient<Func<IksOutDbContext>>(x => x.GetService<IksOutDbContext>);
            services.AddTransient<Func<IksPublishingJobDbContext>>(x => x.GetService<IksPublishingJobDbContext>);

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

            services.AddTransient<IksImportBatchJob>();

            services.AddTransient<Func<IksImportCommand>>(
                x => () => new IksImportCommand(
                    x.GetRequiredService<DkSourceDbContext>(),
                    new IDiagnosticKeyProcessor[0]
                    )
                );

            services.AddTransient<IksEngine>();

            services.AddTransient<IksInputSnapshotCommand>();
            services.AddTransient<IksFormatter>();
            services.AddSingleton<IIksConfig, IksConfig>();
            services.AddTransient<MarkDiagnosisKeysAsUsedByIks>();
            services.AddTransient<IksJobContentWriter>();

            services.ManifestForV3Startup();
        }
    }
}
