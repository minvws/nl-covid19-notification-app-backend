// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine
{
    public class Program
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
            var run = new List<Action>();

            //TODO read EFGS run.

            var c10 = serviceProvider.GetRequiredService<SnapshotWorkflowTeksToDksCommand>();
            run.Add(() => c10.ExecuteAsync().GetAwaiter().GetResult());

            var eksEngineSettings = serviceProvider.GetRequiredService<IEksEngineConfig>();
            if (eksEngineSettings.IksImportEnabled)
            {
                var c20 = serviceProvider.GetRequiredService<IksImportBatchJob>();
                run.Add(() => c20.ExecuteAsync().GetAwaiter().GetResult());
            }
            else
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("IksImport is disabled; Iks files will not be processed.");
            }

            var c30 = serviceProvider.GetRequiredService<ExposureKeySetBatchJobMk3>();
            run.Add(() => c30.ExecuteAsync().GetAwaiter().GetResult());

            var c40 = serviceProvider.GetRequiredService<ManifestUpdateCommand>();
            run.Add(() => c40.ExecuteAllAsync().GetAwaiter().GetResult());

            var c50 = serviceProvider.GetRequiredService<NlContentResignExistingV1ContentCommand>();
            run.Add(() => c50.ExecuteAsync().GetAwaiter().GetResult());

            var c55 = serviceProvider.GetRequiredService<RemovePublishedDiagnosisKeys>();
            run.Add(() => c55.Execute());

            var c60 = serviceProvider.GetService<RemoveDuplicateDiagnosisKeysForIksWithSpCommand>();
            run.Add(() => c60.ExecuteAsync().GetAwaiter().GetResult());

            var c35 = serviceProvider.GetRequiredService<IksEngine>();
            run.Add(() => c35.ExecuteAsync().GetAwaiter().GetResult());

            //TODO write EFGS run.

            foreach (var i in run)
                i();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            //Databases
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


            //Services
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            //Configuration
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddSingleton<IAcceptableCountriesSetting, EfgsInteropConfig>();
            services.AddSingleton<IOutboundFixedCountriesOfInterestSetting, EfgsInteropConfig>();
            services.AddSingleton<IEksEngineConfig, EfgsInteropConfig>();

            //EKS Engine
            services.EksEngine();

            services.AddTransient<RemoveDuplicateDiagnosisKeysForIksWithSpCommand>();
            services.AddTransient<RemovePublishedDiagnosisKeys>();

            services.AddSingleton<EksBuilderV1LoggingExtensions>();
            services.AddSingleton<ResignerLoggingExtensions>();
            services.AddSingleton<EksEngineLoggingExtensions>();
            services.AddSingleton<SnapshotLoggingExtensions>();
            services.AddSingleton<EksJobContentWriterLoggingExtensions>();
            services.AddSingleton<MarkWorkFlowTeksAsUsedLoggingExtensions>();
            services.AddSingleton<ManifestUpdateCommandLoggingExtensions>();
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();

            services.AddTransient<IRiskCalculationParametersReader, RiskCalculationParametersHardcoded>();
            services.AddTransient<IInfectiousness>(
                x =>
                {
                    var rr = x.GetService<IRiskCalculationParametersReader>();
                    var days = rr.GetInfectiousDaysAsync();
                    return new Infectiousness(days);
                }
            );

            //Signing
            services.NlResignerStartup();
            services.DummySignerStartup();
            services.GaSignerStartup();

            services.AddTransient<IksImportBatchJob>();
            services.AddTransient<Func<IksImportCommand>>(x => x.GetRequiredService<IksImportCommand>);
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
                )
            );

            services.AddTransient<OnlyIncludeCountryOfOriginKeyProcessor>();
            services.AddTransient<DosDecodingDiagnosticKeyProcessor>();
            services.AddTransient<NlTrlFromDecodedDosDiagnosticKeyProcessor>();
            services.AddTransient<ExcludeTrlNoneDiagnosticKeyProcessor>();
            services.AddTransient<NlSymptomaticFromDecodedDosDiagnosticKeyProcessor>();

            services.AddTransient<IksEngine>();

            services.AddTransient<IksInputSnapshotCommand>();
            services.AddTransient<IksFormatter>();
            services.AddSingleton<IIksConfig, IksConfig>();
            services.AddTransient<MarkDiagnosisKeysAsUsedByIks>();
            services.AddTransient<IksJobContentWriter>();

            //ManifestEngine
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
