// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader.ServiceRegistrations
{
    public static class PublishingSetup
    {
        public static void PublishingRegistration(this IServiceCollection services)
        {
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddHttpClient<IHsmSignerService, HsmSignerService>();
            services.AddTransient<ContentValidator>();
            services.AddTransient<ContentInsertDbCommand>();
        }
    }
}
