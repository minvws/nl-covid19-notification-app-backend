// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;

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
            services.AddTransient<RemoveExpiredManifestsCommand>();
            services.AddTransient<RemoveExpiredIksInCommand>();
            services.AddTransient<RemoveExpiredIksOutCommand>();

            services.AddTransient<RemoveExpiredManifestsReceiver>();
        }
    }
}
