// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public static class ServiceRegistrationHelpers
    {
        public static void EksEngine(this IServiceCollection services)
        {
            //services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddTransient<ExposureKeySetBatchJobMk3>();
            services.AddTransient<IEksStuffingGeneratorMk2, EksStuffingGeneratorMk2>();
            services.AddTransient<ITransmissionRiskLevelCalculationMk2, TransmissionRiskLevelCalculationMk2>();
            services.AddTransient<EksBuilderV1>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();

            services.AddTransient<IInfectiousness>(
                x =>
                {
                    var rr = x.GetService<IRiskCalculationParametersReader>();
                    var days = rr.GetInfectiousDaysAsync();
                    return new Infectiousness(days);
                }
            );

            services.AddTransient(x => new SnapshotWorkflowTeksToDksCommand(
                x.GetRequiredService<ILoggerFactory>().CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                x.GetRequiredService<IUtcDateTimeProvider>(),
                x.GetRequiredService<ITransmissionRiskLevelCalculationMk2>(),
                x.GetRequiredService<WorkflowDbContext>(),
                x.GetRequiredService<Func<WorkflowDbContext>>(),
                x.GetRequiredService<Func<DkSourceDbContext>>(),
                x.GetRequiredService<IWrappedEfExtensions>(),
                new IDiagnosticKeyProcessor[]
                {
                    x.GetRequiredService<ExcludeTrlNoneDiagnosticKeyProcessor>(),
                    x.GetRequiredService<FixedCountriesOfInterestOutboundDiagnosticKeyProcessor>(),
                    x.GetRequiredService<NlToEfgsDsosDiagnosticKeyProcessorMk1>()
                }
             ));

            services.AddTransient<ISnapshotEksInput, SnapshotDiagnosisKeys>();
            services.AddTransient<IEksJobContentWriter, EksJobContentWriter>();
            services.AddTransient<MarkDiagnosisKeysAsUsedLocally>();
            services.AddTransient<IWriteStuffingToDiagnosisKeys>(x =>
                new WriteStuffingToDiagnosisKeys(
                    x.GetRequiredService<DkSourceDbContext>(),
                    x.GetRequiredService<EksPublishingJobDbContext>(),
                    new IDiagnosticKeyProcessor[]
                    {
                        x.GetRequiredService<FixedCountriesOfInterestOutboundDiagnosticKeyProcessor>(),
                        x.GetRequiredService<NlToEfgsDsosDiagnosticKeyProcessorMk1>()
                    }));

            services.AddSingleton<EfgsInteropConfig>();
            services.AddSingleton<IOutboundFixedCountriesOfInterestSetting>(x => x.GetRequiredService<EfgsInteropConfig>());
            services.AddTransient<FixedCountriesOfInterestOutboundDiagnosticKeyProcessor>();
            services.AddTransient<NlToEfgsDsosDiagnosticKeyProcessorMk1>();
            services.AddTransient<ExcludeTrlNoneDiagnosticKeyProcessor>();

            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<GeneratedProtobufEksContentFormatter>();
            services.AddTransient<IEksBuilder, EksBuilderV1>();
            services.AddTransient<IEksContentFormatter, GeneratedProtobufEksContentFormatter>();
            services.AddTransient<ISnapshotEksInput, SnapshotDiagnosisKeys>();
        }
    }
}
