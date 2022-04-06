// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader.Jobs;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader.ServiceRegistrations
{
    public static class JobsSetup
    {
        public static void JobsRegistration(this IServiceCollection services)
        {
            services.AddTransient<DashboardDataDownloadJob>();
            services.AddTransient<DashboardDataProcessJob>();
            services.AddTransient<DashboardDataPublishingJob>();
        }
    }
}