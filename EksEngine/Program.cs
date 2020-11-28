// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ServiceRegHelpers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

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
            catch(Exception)
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

            var c20 = serviceProvider.GetRequiredService<IksImportBatchJob>();
            run.Add(() => c20.ExecuteAsync().GetAwaiter().GetResult());

            var c30 = serviceProvider.GetRequiredService<ExposureKeySetBatchJobMk3>();
            run.Add(() => c30.ExecuteAsync().GetAwaiter().GetResult());

            var c40 = serviceProvider.GetRequiredService<ManifestUpdateCommand>();
            run.Add(() => c40.ExecuteAsync().GetAwaiter().GetResult());

            var c50 = serviceProvider.GetRequiredService<NlContentResignExistingV1ContentCommand>();
            run.Add(() => c50.ExecuteAsync().GetAwaiter().GetResult());

            var c35 = serviceProvider.GetRequiredService<IksEngine>();
            run.Add(() => c35.ExecuteAsync().GetAwaiter().GetResult());

            //TODO write EFGS run.

            foreach (var i in run)
                i();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            //Databases
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


            //Services
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            //Configuration
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddSingleton<IAcceptableCountriesSetting, EfgsInteropConfig>();
            services.AddSingleton<IOutboundFixedCountriesOfInterestSetting, EfgsInteropConfig>();

            //EKS Engine
            services.EksEngine();

            services.AddTransient<ManifestUpdateCommand>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<ManifestBuilder>();
            
            services.AddSingleton<EksBuilderV1LoggingExtensions>();
            services.AddSingleton<ResignerLoggingExtensions>();
            services.AddSingleton<EksEngineLoggingExtensions>();
            services.AddSingleton<SnapshotLoggingExtensions>();
            services.AddSingleton<EksJobContentWriterLoggingExtensions>();
            services.AddSingleton<MarkWorkFlowTeksAsUsedLoggingExtensions>();
            services.AddSingleton<ManifestUpdateCommandLoggingExtensions>();
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();

            //Manifest Engine
            services.ManifestEngine();

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

            services.AddTransient<IksEngine>();

            services.AddTransient < IksInputSnapshotCommand>(); 
            services.AddTransient < IksFormatter>();
            services.AddSingleton<IIksConfig, IksConfig>();
            services.AddTransient < MarkDiagnosisKeysAsUsedByIks>();
            services.AddTransient < IksJobContentWriter>();

            services.ManifestForV3Startup();
        }
    }
}
