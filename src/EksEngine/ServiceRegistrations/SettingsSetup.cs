// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.ServiceRegistrations
{
    public static class SettingsSetup
    {
        public static void SettingsRegistration(this IServiceCollection services, IConfigurationRoot configuration)
        {
            //Services
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();

            //Configuration
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddSingleton<IHsmSignerConfig, HsmSignerConfig>(
                x => new HsmSignerConfig(configuration, "Certificates:HsmSigner")
            );

            services.AddSingleton<IAcceptableCountriesSetting, EfgsInteropConfig>();
            services.AddSingleton<IOutboundFixedCountriesOfInterestSetting, EfgsInteropConfig>();
            services.AddSingleton<IEksEngineConfig, EfgsInteropConfig>();

            services.AddTransient<IRiskCalculationParametersReader, RiskCalculationParametersHardcoded>();
            services.AddTransient<IInfectiousness>(
                x =>
                {
                    var rr = x.GetService<IRiskCalculationParametersReader>();
                    var days = rr.GetInfectiousDaysAsync();
                    return new Infectiousness(days);
                }
            );
        }
    }
}
