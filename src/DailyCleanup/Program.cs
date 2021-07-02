// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
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
            run.Add(() => c60.Execute());

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

            var c125 = serviceProvider.GetRequiredService<RemovePublishedDiagnosisKeys>();
            run.Add(() => c125.Execute());

            var c126 = serviceProvider.GetRequiredService<RemoveDiagnosisKeysReadyForCleanup>();
            run.Add(() => c126.ExecuteAsync().GetAwaiter().GetResult());

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

            services.AddSingleton<IWrappedEfExtensions, SqlServerWrappedEfExtensions>();

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.EksEngine();

            services.AddTransient<RemoveExpiredManifestsReceiver>();
            services.AddTransient<RemoveExpiredManifestsCommand>();

            services.AddTransient<RemoveExpiredEksCommand>();
            services.AddTransient<RemoveExpiredEksV2Command>();
            services.AddTransient<RemoveExpiredWorkflowsCommand>();
            services.AddTransient<RemoveDuplicateDiagnosisKeysForIksWithSpCommand>();
            services.AddTransient<RemovePublishedDiagnosisKeys>();
            services.AddTransient<RemoveDiagnosisKeysReadyForCleanup>();

            services.AddSingleton<IManifestConfig, ManifestConfig>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();
            services.AddSingleton<IAcceptableCountriesSetting, EfgsInteropConfig>();
            services.AddSingleton<IOutboundFixedCountriesOfInterestSetting, EfgsInteropConfig>();

            services.AddSingleton<EksBuilderV1LoggingExtensions>();
            services.AddSingleton<DailyCleanupLoggingExtensions>();

            services.AddSingleton<ExpiredEksLoggingExtensions>();
            services.AddSingleton<ExpiredWorkflowLoggingExtensions>();
            services.AddSingleton<ResignerLoggingExtensions>();
            services.AddSingleton<ExpiredEksV2LoggingExtensions>();

            services.AddSingleton<EksEngineLoggingExtensions>();
            services.AddSingleton<SnapshotLoggingExtensions>();
            services.AddSingleton<EksJobContentWriterLoggingExtensions>();
            services.AddSingleton<MarkWorkFlowTeksAsUsedLoggingExtensions>();
            services.AddSingleton<ManifestUpdateCommandLoggingExtensions>();
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();

            services.NlResignerStartup();
            services.DummySignerStartup();
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
                        x.GetRequiredService<ExcludeTrlNoneDiagnosticKeyProcessor>()
                    },
                    x.GetRequiredService<ITekValidatorConfig>(),
                    x.GetRequiredService<IUtcDateTimeProvider>(),
                    x.GetRequiredService<ILogger<IksImportCommand>>()
                    ));

            services.AddTransient<OnlyIncludeCountryOfOriginKeyProcessor>();
            services.AddTransient<DosDecodingDiagnosticKeyProcessor>();
            services.AddTransient<NlTrlFromDecodedDosDiagnosticKeyProcessor>();
            services.AddTransient<ExcludeTrlNoneDiagnosticKeyProcessor>();

            services.AddTransient<Func<IksImportCommand>>(x => x.GetRequiredService<IksImportCommand>);
            services.AddTransient<IRiskCalculationParametersReader, RiskCalculationParametersHardcoded>();
            services.AddTransient<IInfectiousness>(
                x =>
                {
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
            services.AddTransient(x =>
                SignerConfigStartup.BuildGaSigner(
                    x.GetRequiredService<IConfiguration>(),
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()));
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
        }
    }

}
