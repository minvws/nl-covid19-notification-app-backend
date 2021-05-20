// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;
using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

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
            run.Add(() => c40.ExecuteAllAsync().GetAwaiter().GetResult());

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

            var c121 = serviceProvider.GetRequiredService<RemoveExpiredManifestsV3Command>();
            run.Add(() => logger.WriteManifestV3CleanupStarting());
            run.Add(() => c121.ExecuteAsync().GetAwaiter().GetResult());

            var c122 = serviceProvider.GetRequiredService<RemoveExpiredManifestsV4Command>();
            run.Add(() => logger.WriteManifestV4CleanupStarting());
            run.Add(() => c122.ExecuteAsync().GetAwaiter().GetResult());

            var c125 = serviceProvider.GetRequiredService<RemovePublishedDiagnosisKeys>();
            run.Add(() => c125.Execute());

            var c130 = serviceProvider.GetRequiredService<RemoveDuplicateDiagnosisKeysForIksWithSpCommand>();
            run.Add(() => c130.ExecuteAsync().GetAwaiter().GetResult());

            var c140 = serviceProvider.GetRequiredService<RemoveExpiredIksInCommand>();
            run.Add(() => c140.ExecuteAsync().GetAwaiter().GetResult());

            var c150 = serviceProvider.GetRequiredService<RemoveExpiredIksOutCommand>();
            run.Add(() => c150.ExecuteAsync().GetAwaiter().GetResult());

            var c100 = serviceProvider.GetRequiredService<NlContentResignExistingV1ContentCommand>();
            run.Add(() => logger.WriteResignerStarting());
            run.Add(() => c100.ExecuteAsync().GetAwaiter().GetResult());

            run.Add(() => logger.WriteFinished());

            foreach (var i in run)
                i();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient(x => x.CreateDbContext(y => new WorkflowDbContext(y), DatabaseConnectionStringNames.Workflow, false));
            services.AddTransient(x => x.CreateDbContext(y => new ContentDbContext(y), DatabaseConnectionStringNames.Content, false));
            services.AddTransient(x => x.CreateDbContext(y => new EksPublishingJobDbContext(y), DatabaseConnectionStringNames.EksPublishing, false));
            services.AddTransient(x => x.CreateDbContext(y => new StatsDbContext(y), DatabaseConnectionStringNames.Stats, false));
            services.AddTransient(x => x.CreateDbContext(y => new DkSourceDbContext(y), DatabaseConnectionStringNames.DkSource, false));
            services.AddTransient(x => x.CreateDbContext(y => new IksInDbContext(y), DatabaseConnectionStringNames.IksIn, false));
            services.AddTransient(x => x.CreateDbContext(y => new IksPublishingJobDbContext(y), DatabaseConnectionStringNames.IksPublishing, false));
            services.AddTransient(x => x.CreateDbContext(y => new IksOutDbContext(y), DatabaseConnectionStringNames.IksOut, false));

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
            
            services.AddTransient<RemoveExpiredManifestsCommand>();
            services.AddTransient<RemoveExpiredManifestsV2Command>();
            services.AddTransient<RemoveExpiredManifestsV3Command>();
            services.AddTransient<RemoveExpiredManifestsV4Command>();
            services.AddTransient<RemoveExpiredEksCommand>();
            services.AddTransient<RemoveExpiredEksV2Command>();
            services.AddTransient<RemoveExpiredWorkflowsCommand>();
            services.AddTransient<RemoveDuplicateDiagnosisKeysForIksWithSpCommand>();
            services.AddTransient<RemovePublishedDiagnosisKeys>();

            services.AddSingleton<IManifestConfig, ManifestConfig>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();
            services.AddSingleton<IAcceptableCountriesSetting, EfgsInteropConfig>();
            services.AddSingleton<IOutboundFixedCountriesOfInterestSetting, EfgsInteropConfig>();

            services.AddSingleton<EksBuilderV1LoggingExtensions>();
            services.AddSingleton<DailyCleanupLoggingExtensions>();
            services.AddSingleton<ExpiredManifestLoggingExtensions>();
            services.AddSingleton<ExpiredEksLoggingExtensions>();
            services.AddSingleton<ExpiredWorkflowLoggingExtensions>();
            services.AddSingleton<ResignerLoggingExtensions>();
            services.AddSingleton<ExpiredEksV2LoggingExtensions>();
            services.AddSingleton<ExpiredManifestV2LoggingExtensions>();
            services.AddSingleton<ExpiredManifestV3LoggingExtensions>();
            services.AddSingleton<ExpiredManifestV4LoggingExtensions>();
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

            services.AddTransient(
                x => new IksImportCommand(
                    x.GetRequiredService<DkSourceDbContext>(),
                    new IDiagnosticKeyProcessor[]
                    {
                        x.GetRequiredService<OnlyIncludeCountryOfOriginKeyProcessor>(),
                        x.GetRequiredService<DosDecodingDiagnosticKeyProcessor>(), //Adds result to metadata
                        x.GetRequiredService<NlTrlFromDecodedDosDiagnosticKeyProcessor>(),
                        x.GetRequiredService<ExcludeTrlNoneDiagnosticKeyProcessor>(),
                        x.GetRequiredService<NlSymptomaticFromDecodedDosDiagnosticKeyProcessor>(),
                    },
                    x.GetRequiredService<ITekValidatorConfig>(),
                    x.GetRequiredService<IUtcDateTimeProvider>(),
                    x.GetRequiredService<ILogger<IksImportCommand>>()
                    ));

            services.AddTransient<OnlyIncludeCountryOfOriginKeyProcessor>();
            services.AddTransient<DosDecodingDiagnosticKeyProcessor>();
            services.AddTransient<NlTrlFromDecodedDosDiagnosticKeyProcessor>();
            services.AddTransient<ExcludeTrlNoneDiagnosticKeyProcessor>();
            services.AddTransient<NlSymptomaticFromDecodedDosDiagnosticKeyProcessor>();

            services.AddTransient<Func<IksImportCommand>>(x => x.GetRequiredService<IksImportCommand>);
            services.AddTransient<IRiskCalculationParametersReader, RiskCalculationParametersHardcoded>();
            services.AddTransient<IInfectiousness>(
                x => {
                    var rr = x.GetService<IRiskCalculationParametersReader>();
                    var days = rr.GetInfectiousDaysAsync();
                    return new Infectiousness(days);
                }
            );

            services.AddTransient<IksEngine>();

            services.AddTransient<IksInputSnapshotCommand>();
            services.AddTransient<IksFormatter>();
            services.AddSingleton<IIksConfig, IksConfig>();
            services.AddTransient<MarkDiagnosisKeysAsUsedByIks>();
            services.AddTransient<IksJobContentWriter>();

            services.AddSingleton<IIksCleaningConfig, IksCleaningConfig>();
            services.AddSingleton<RemoveExpiredIksLoggingExtensions>();
            services.AddTransient<RemoveExpiredIksInCommand>();
            services.AddTransient<RemoveExpiredIksOutCommand>();

            // ManifestEngine
            services.AddTransient<ManifestUpdateCommand>();
            services.AddTransient<ManifestV2Builder>();
            services.AddTransient<ManifestV3Builder>();
            services.AddTransient<ManifestV4Builder>();
            services.AddTransient<Func<IContentEntityFormatter>>(x => x.GetRequiredService<StandardContentEntityFormatter>);
            services.AddTransient<StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient(x =>
                SignerConfigStartup.BuildEvSigner(
                    x.GetRequiredService<IConfiguration>(),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>(),
                    x.GetRequiredService<IUtcDateTimeProvider>()));
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
        }
    }

}
