// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.DiagnosisKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Eks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Iks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Jobs;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.ServiceRegistrations
{
    public static class DailyCleanupJobSetup
    {
        public static void DailyCleanupJobRegistration(this IServiceCollection services)
        {
            services.AddTransient<DailyCleanupJob>();

            services.AddTransient<RemoveExpiredWorkflowsCommand>();
            services.AddTransient<RemoveDiagnosisKeysReadyForCleanupCommand>();
            services.AddTransient<RemovePublishedDiagnosisKeysCommand>();
            services.AddTransient<RemoveExpiredEksCommand>();
            services.AddTransient<RemoveExpiredEksV2Command>();
            services.AddTransient<RemoveExpiredEksV3Command>();
            services.AddTransient<RemoveExpiredManifestsCommand>();
            services.AddTransient<RemoveExpiredIksInCommand>();
            services.AddTransient<RemoveExpiredIksOutCommand>();

            services.AddTransient<RemoveExpiredManifestsReceiver>();
        }
    }
}
