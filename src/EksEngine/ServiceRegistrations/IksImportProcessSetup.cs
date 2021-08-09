// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.ServiceRegistrations
{
    public static class IksImportProcessSetup
    {
        public static void IksImportProcessRegistration(this IServiceCollection services)
        {
            // Batch Job
            services.AddTransient<IksImportBatchJob>();

            services.AddTransient<OnlyIncludeCountryOfOriginKeyProcessor>();
            services.AddTransient<DosDecodingDiagnosticKeyProcessor>();
            services.AddTransient<NlTrlFromDecodedDosDiagnosticKeyProcessor>();
            services.AddTransient<ExcludeTrlNoneDiagnosticKeyProcessor>();

            services.AddTransient<Func<IksImportCommand>>(x => x.GetRequiredService<IksImportCommand>);
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
                )
            );
        }
    }
}
