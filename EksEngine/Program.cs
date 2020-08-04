// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
        }

        private static void Start(IServiceProvider serviceProvider, string[] args)
        {
            var useAllKeys = args.Length > 0 && bool.TryParse(args[0], out var value) && value;
            using var job = serviceProvider.GetRequiredService<ExposureKeySetBatchJobMk3>();
            job.Execute(useAllKeys).GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddLogging(builder =>
            {
              builder.AddSerilog(new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger(), true);
            });

            services.AddScoped(x => DbContextStartup.Workflow(x, false));
            services.AddScoped(x => DbContextStartup.Content(x, false));
            services.AddScoped(x => DbContextStartup.Publishing(x, false));

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksHeaderInfoConfig, EksHeaderInfoConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddTransient<ITransmissionRiskLevelCalculation, TransmissionRiskLevelCalculationV1>();
            services.AddTransient<ExposureKeySetBatchJobMk3>();
            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddTransient<IEksStuffingGenerator, EksStuffingGenerator>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<EksBuilderV1>();
            services.AddTransient<GeneratedProtobufEksContentFormatter>();
            services.AddTransient<IEksBuilder, EksBuilderV1>();
            services.AddTransient<IEksContentFormatter, GeneratedProtobufEksContentFormatter>();

            services.NlSignerStartup(configuration.UseCertificatesFromResources());
            services.GaSignerStartup(configuration.UseCertificatesFromResources());

        }
    }
}
