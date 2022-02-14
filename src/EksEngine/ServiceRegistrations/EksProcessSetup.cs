// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.ServiceRegistrations
{
    public static class EksProcessSetup
    {
        public static void EksProcessRegistration(this IServiceCollection services)
        {
            // Batch Job
            services.AddTransient<EksBatchJob>();

            // Workflow to DiagnosisKeys
            services.AddTransient(x => new SnapshotWorkflowTeksToDksCommand(
                x.GetRequiredService<ILoggerFactory>().CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                x.GetRequiredService<IUtcDateTimeProvider>(),
                x.GetRequiredService<ITransmissionRiskLevelCalculationMk2>(),
                x.GetRequiredService<WorkflowDbContext>(),
                x.GetRequiredService<DkSourceDbContext>(),
                new IDiagnosticKeyProcessor[]
                {
                    x.GetRequiredService<ExcludeTrlNoneDiagnosticKeyProcessor>(),
                    x.GetRequiredService<FixedCountriesOfInterestOutboundDiagnosticKeyProcessor>(),
                    x.GetRequiredService<NlToEfgsDsosDiagnosticKeyProcessorMk1>()
                }
            ));

            // DiagnosisKeys to Eks publishing job
            services.AddTransient<RemoveDuplicateDiagnosisKeysCommand>();
            services.AddTransient<ExposureKeySetBatchJobMk3>();
            services.AddTransient<IEksStuffingGeneratorMk2, EksStuffingGeneratorMk2>();
            services.AddTransient<ITransmissionRiskLevelCalculationMk2, TransmissionRiskLevelCalculationMk2>();
            services.AddTransient<ISnapshotEksInput, SnapshotDiagnosisKeys>();

            // Publishing
            services.AddTransient<IEksContentFormatter, GeneratedProtobufEksContentFormatter>();
            services.AddTransient<IEksJobContentWriter, EksJobContentWriter>();

            services.AddTransient<IEksBuilder, EksBuilderV1>(x => new EksBuilderV1(
                x.GetRequiredService<IEksHeaderInfoConfig>(),
                SignerConfigStartup.BuildGaSigner(
                    x.GetRequiredService<ILogger<LocalMachineStoreCertificateProvider>>(),
                    x.GetRequiredService<IConfiguration>()),
                SignerConfigStartup.BuildGaV15Signer(
                    x.GetRequiredService<ILogger<LocalMachineStoreCertificateProvider>>(),
                    x.GetRequiredService<IConfiguration>()),
                SignerConfigStartup.BuildEvSigner(
                    x.GetRequiredService<IConfiguration>(),
                    x.GetRequiredService<ILogger<LocalMachineStoreCertificateProvider>>(),
                    x.GetRequiredService<IUtcDateTimeProvider>()),
                x.GetRequiredService<IUtcDateTimeProvider>(),
                x.GetRequiredService<IEksContentFormatter>(),
                x.GetRequiredService<ILogger<EksBuilderV1>>()
            ));

            services.AddTransient<MarkDiagnosisKeysAsUsedLocally>();
            services.AddTransient<FixedCountriesOfInterestOutboundDiagnosticKeyProcessor>();
            services.AddTransient<NlToEfgsDsosDiagnosticKeyProcessorMk1>();

            services.AddTransient<IWriteStuffingToDiagnosisKeys>(x =>
                new WriteStuffingToDiagnosisKeys(
                    x.GetRequiredService<DkSourceDbContext>(),
                    x.GetRequiredService<EksPublishingJobDbContext>(),
                    new IDiagnosticKeyProcessor[]
                    {
                        x.GetRequiredService<FixedCountriesOfInterestOutboundDiagnosticKeyProcessor>(),
                        x.GetRequiredService<NlToEfgsDsosDiagnosticKeyProcessorMk1>()
                    }));
        }
    }
}
