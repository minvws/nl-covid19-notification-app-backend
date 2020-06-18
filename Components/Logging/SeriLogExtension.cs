// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging
{
    public static class SeriLogExtension
    {
        public static void AddSeriLog(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            // Initialize serilog logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            serviceCollection.AddLogging(logger =>
            {
                logger.AddSerilog(dispose: true);
            });
        }
    }
}
