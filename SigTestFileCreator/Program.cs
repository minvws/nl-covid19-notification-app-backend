// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace SigTestFileCreator
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

    internal class Program
    {
        public static int Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
            return 0;
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var job = serviceProvider.GetRequiredService<SigTesterService>();
            job.Execute().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<SigTesterService>();
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddTransient<IEksContentFormatter, GeneratedProtobufEksContentFormatter>();
            services.AddTransient<IEksBuilder, EksBuilderV1>();
            services.NlSignerStartup();
            services.GaSignerStartup();
        }
    }
}
