// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.ServiceRegistrations
{
    public static class LoggingExtensionsSetup
    {
        public static void LoggingExtensionsRegistration(this IServiceCollection services)
        {
            services.AddSingleton<EksBuilderV1LoggingExtensions>();
            services.AddSingleton<SnapshotLoggingExtensions>();
            services.AddSingleton<EksJobContentWriterLoggingExtensions>();
            services.AddSingleton<ManifestUpdateCommandLoggingExtensions>();
        }
    }
}
