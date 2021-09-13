// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.ServiceRegistrations
{
    public static class StatisticsSetup
    {
        public static void DailyStatsRegistration(this IServiceCollection services)
        {
            services.AddScoped<TotalWorkflowCountStatsQueryCommand>();
            services.AddScoped<TotalWorkflowsWithTeksQueryCommand>();
            services.AddScoped<TotalWorkflowAuthorisedCountStatsQueryCommand>();
            services.AddScoped<PublishedTekCountStatsQueryCommand>();
            services.AddScoped<TotalTekCountStatsQueryCommand>();
            services.AddScoped<IStatisticsWriter, StatisticsDbWriter>();
            services.AddScoped<StatisticsCommand>(x =>
                new StatisticsCommand(x.GetRequiredService<IStatisticsWriter>(),
                    new IStatsQueryCommand[] {
                        x.GetRequiredService<TotalWorkflowCountStatsQueryCommand>(),
                        x.GetRequiredService<TotalWorkflowsWithTeksQueryCommand>(),
                        x.GetRequiredService<TotalWorkflowAuthorisedCountStatsQueryCommand>(),
                        x.GetRequiredService<PublishedTekCountStatsQueryCommand>(),
                        x.GetRequiredService<TotalTekCountStatsQueryCommand>(),
                    })
            );
        }

    }
}
