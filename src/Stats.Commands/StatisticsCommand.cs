// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands
{
    public class StatisticsCommand : IStatisticsCommand
    {
        private readonly IStatisticsWriter _writer;
        private readonly IStatsQueryCommand[] _statsQueries;

        public StatisticsCommand(IStatisticsWriter writer, IStatsQueryCommand[] statsQueries)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _statsQueries = statsQueries ?? throw new ArgumentNullException(nameof(statsQueries));
        }

        public void Execute()
        {
            var stats = _statsQueries.Select(x => x.Execute()).ToArray();
            _writer.Write(stats);
        }
    }
}
