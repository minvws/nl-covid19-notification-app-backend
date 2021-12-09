// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Eks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Iks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.ServiceRegistrations
{
    public static class LoggingExtensionsSetup
    {
        public static void LoggingExtensionsRegistration(this IServiceCollection services)
        {
            services.AddSingleton<RemoveExpiredIksLoggingExtensions>();
        }
    }
}
