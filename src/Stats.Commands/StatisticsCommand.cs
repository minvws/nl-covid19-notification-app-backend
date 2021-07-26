// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands
{
    public class StatisticsCommand : BaseCommand
    {
        private readonly IStatisticsWriter _writer;
        private readonly IStatsQueryCommand[] _statsQueries;

        public StatisticsCommand(IStatisticsWriter writer, IStatsQueryCommand[] statsQueries)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _statsQueries = statsQueries ?? throw new ArgumentNullException(nameof(statsQueries));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            var results = new List<StatisticArgs>();
            foreach (var iStatsQueryCommand in _statsQueries)
            {
                var result = await iStatsQueryCommand.ExecuteAsync();
                results.Add(result);
            }

            await _writer.WriteAsync(results.ToArray());

            return null;
        }
    }
}
