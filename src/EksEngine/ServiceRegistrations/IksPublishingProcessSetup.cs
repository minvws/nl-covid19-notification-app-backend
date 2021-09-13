// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.ServiceRegistrations
{
    public static class IksPublishingProcessSetup
    {
        public static void IksPublishingProcessRegistration(this IServiceCollection services)
        {
            // Batch Job
            services.AddTransient<IksBatchJob>();

            // config & settings
            services.AddSingleton<IIksConfig, IksConfig>();
            services.AddTransient<IksFormatter>();

            // commands
            services.AddTransient<IksInputSnapshotCommand>();
            services.AddTransient<MarkDiagnosisKeysAsUsedByIks>();
            services.AddTransient<IksJobContentWriter>();
            services.AddTransient<IksEngine>();
        }
    }
}
