// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands
{
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
}