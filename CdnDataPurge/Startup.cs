// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using CdnDataPurge;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace CdnDataPurge
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ComponentsContainerHelper.RegisterDefaultServices(services);

            services.AddScoped(x =>
            {
                var builder = new SqlServerDbContextOptionsBuilder(Environment.GetEnvironmentVariable("ConnectionStrings:Content"));
                var result = new ExposureContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped<ICdnContentConfig, CdnContentConfig>();
            services.AddScoped<CdnContentPurgeCommand, CdnContentPurgeCommand>();
        }
    }
}
