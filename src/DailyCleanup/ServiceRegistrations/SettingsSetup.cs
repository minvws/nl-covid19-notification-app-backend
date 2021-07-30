// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.ServiceRegistrations
{
    public static class SettingsSetup
    {
        public static void SettingsRegistration(this IServiceCollection services)
        {
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IIksCleaningConfig, IksCleaningConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();
            services.AddSingleton<IManifestConfig, ManifestConfig>();
            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();
            services.AddSingleton<IAcceptableCountriesSetting, EfgsInteropConfig>();
            services.AddSingleton<IOutboundFixedCountriesOfInterestSetting, EfgsInteropConfig>();

        }
    }
}
