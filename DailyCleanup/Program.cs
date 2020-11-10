// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DailyCleanup;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksBuilderV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredEks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredEksV2;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredManifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredManifestV2;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredWorkflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.Resigner;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.Snapshot;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Expiry;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine
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

            logger.WriteEksEngineStarting();
            j1.Execute().GetAwaiter().GetResult();
            
            logger.WriteManifestEngineStarting();
            j2.Execute().GetAwaiter().GetResult();
            
            logger.WriteDailyStatsCalcStarting();
            j6.Execute();
            
            logger.WriteManiFestCleanupStarting();
            j3.Execute().GetAwaiter().GetResult();
            
            logger.WriteEksCleanupStarting();
            j4.Execute();
            
            logger.WriteWorkflowCleanupStarting();
            j5.Execute();
            
            logger.WriteResignerStarting();
            j7.Execute().GetAwaiter().GetResult();
            
            logger.WriteEksV2CleanupStarting();
            j8.Execute();
            
            logger.WriteManifestV2CleanupStarting();
            j9.Execute();

            logger.WriteFinished();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient(x => DbContextStartup.Workflow(x, false));
            services.AddTransient(x => DbContextStartup.Content(x, false));
            services.AddTransient(x => DbContextStartup.Publishing(x, false));
            services.AddTransient(x => DbContextStartup.Stats(x, false));

            services.AddTransient<Func<WorkflowDbContext>>(x => x.GetService<WorkflowDbContext>);
            services.AddTransient<Func<PublishingJobDbContext>>(x => x.GetService<PublishingJobDbContext>);
            services.AddTransient<Func<ContentDbContext>>(x => x.GetService<ContentDbContext>);

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddTransient<ITransmissionRiskLevelCalculation, TransmissionRiskLevelCalculationV1>();
            services.AddTransient<ExposureKeySetBatchJobMk3>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddTransient<IEksStuffingGenerator, EksStuffingGenerator>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<GeneratedProtobufEksContentFormatter>();
            services.AddTransient<IEksBuilder, EksBuilderV1>();
            services.AddTransient<IEksContentFormatter, GeneratedProtobufEksContentFormatter>();
            services.AddTransient<ISnapshotEksInput, SnapshotEksInputMk1>();
            services.AddTransient<IMarkWorkFlowTeksAsUsed, MarkWorkFlowTeksAsUsed>();
            services.AddTransient<EksJobContentWriter>();

            services.AddTransient<ManifestUpdateCommand>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();

            services.AddTransient<ManifestBuilder>();
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();

            services.AddTransient<RemoveExpiredManifestsCommand>();
            services.AddTransient<RemoveExpiredManifestsV2Command>();
            services.AddTransient<RemoveExpiredEksCommand>();
            services.AddTransient<RemoveExpiredEksV2Command>();
            services.AddTransient<RemoveExpiredWorkflowsCommand>();

            services.AddSingleton<IManifestConfig, ManifestConfig>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();

            services.AddSingleton<LoggingExtensionsEksBuilderV1>();
            services.AddSingleton<DailyCleanupLoggingExtensions>();
            services.AddSingleton<ExpiredManifestLoggingExtensions>();
            services.AddSingleton<ExpiredEksLoggingExtensions>();
            services.AddSingleton<LoggingExtensionsExpiredWorkflow>();
            services.AddSingleton<ResignerLoggingExtensions>();
            services.AddSingleton<ExpiredEksV2LoggingExtensions>();
            services.AddSingleton<ExpiredManifestV2LoggingExtensions>();
            services.AddSingleton<EksEngineLoggingExtensions>();
            services.AddSingleton<SnapshotLoggingExtensions>();
            
            services.NlResignerStartup();

            services.DummySignerStartup();
            services.GaSignerStartup();

            services.DailyStatsStartup();

        }
    }
}
