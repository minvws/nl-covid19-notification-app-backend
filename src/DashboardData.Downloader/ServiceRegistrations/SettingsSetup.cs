// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader.ServiceRegistrations
{
    public static class SettingsSetup
    {
        public static void SettingsRegistration(this IServiceCollection services)
        {
            services.AddTransient<IDashboardDataConfig, DashboardDataConfig>();
        }
    }
}
