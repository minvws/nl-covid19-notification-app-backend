// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksBuilderV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksJobContentWriter;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.LocalMachineStoreCertificateProvider;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ManifestUpdateCommand;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.MarkWorkFlowTeksAsUsed;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.Resigner;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.Snapshot;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using Serilog;

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
            catch(Exception)
            {
                return -1;
            }
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var job = serviceProvider.GetRequiredService<ExposureKeySetBatchJobMk3>();
            job.Execute().GetAwaiter().GetResult();
            var job2 = serviceProvider.GetRequiredService<ManifestUpdateCommand>();
            job2.Execute().GetAwaiter().GetResult();
            var job3 = serviceProvider.GetRequiredService<NlContentResignExistingV1ContentCommand>();
            job3.Execute().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient(x => DbContextStartup.Workflow(x, false));
            services.AddTransient(x => DbContextStartup.Content(x, false));
            services.AddTransient(x => DbContextStartup.Publishing(x, false));

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
            services.AddTransient<IContentEntityFormatter, StandardContentEntityFormatter>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<ManifestBuilder>();
            
            services.AddSingleton<LoggingExtensionsEksBuilderV1>();
            services.AddSingleton<ResignerLoggingExtensions>();
            services.AddSingleton<EksEngineLoggingExtensions>();
            services.AddSingleton<SnapshotLoggingExtensions>();
            services.AddSingleton<EksJobContentWriterLoggingExtensions>();
            services.AddSingleton<MarkWorkFlowTeksAsUsedLoggingExtensions>();
            services.AddSingleton<ManifestUpdateCommandLoggingExtensions>();
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();

            services.NlResignerStartup();
            services.DummySignerStartup();
            services.GaSignerStartup();
        }
    }
}
