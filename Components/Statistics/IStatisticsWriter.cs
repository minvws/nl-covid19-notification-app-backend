using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics
{

    public interface IStatisticsCommand
    {
        void Execute();
    }

    public static class StatisticsSetup
    {
        public static void SetupDailyStats(this IServiceCollection services)
        {
            services.AddScoped<IStatisticsCommand>(x =>
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

    public class StatisticsCommand: IStatisticsCommand
    {
        private readonly IStatisticsWriter _Writer;
        private readonly IStatsQueryCommand[] _StatsQueries;

        public StatisticsCommand(IStatisticsWriter writer, IStatsQueryCommand[] statsQueries)
        {
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _StatsQueries = statsQueries ?? throw new ArgumentNullException(nameof(statsQueries));
        }

        public void Execute()
        {
            var stats = _StatsQueries.Select(x => x.Execute()).ToArray();
            _Writer.Write(stats);
        }
    }

    public interface IStatisticsWriter
    {
        void Write(StatisticArgs[] args);
    }
}
