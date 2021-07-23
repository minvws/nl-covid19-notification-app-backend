// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class RemoveExpiredEksV2Command : ICommand
    {
        private readonly ContentDbContext _dbContext;
        private readonly IEksConfig _config;
        private readonly IUtcDateTimeProvider _dtp;
        private readonly ExpiredEksV2LoggingExtensions _logger;

        public RemoveExpiredEksV2Command(ContentDbContext dbContext, IEksConfig config, IUtcDateTimeProvider dtp, ExpiredEksV2LoggingExtensions logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ICommandResult> ExecuteAsync()
        {
            var result = new RemoveExpiredEksCommandResult();

            _logger.WriteStart();

            var cutoff = (_dtp.Snapshot - TimeSpan.FromDays(_config.LifetimeDays)).Date;

            await using (var tx = _dbContext.BeginTransaction())
            {
                result.Found = _dbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2);
                _logger.WriteCurrentEksFound(result.Found);

                var zombies = _dbContext.Content
                    .Where(x => x.Type == ContentTypes.ExposureKeySetV2 && x.Release < cutoff)
                    .Select(x => new { x.PublishingId, x.Release })
                    .ToList();

                result.Zombies = zombies.Count;

                _logger.WriteTotalEksFound(cutoff, result.Zombies);
                foreach (var i in zombies)
                {
                    _logger.WriteEntryFound(i.PublishingId, i.Release);
                }

                if (!_config.CleanupDeletesData)
                {
                    _logger.WriteFinishedNothingRemoved();
                    result.Remaining = result.Found;
                    return result;
                }

                result.GivenMercy = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM [Content] WHERE [Type] = {ContentTypes.ExposureKeySetV2} AND [Release] < {cutoff}");
                await tx.CommitAsync();
                result.Remaining = _dbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2);
            }

            _logger.WriteRemovedAmount(result.GivenMercy, result.Remaining);

            if (result.Reconciliation != 0)
            {
                _logger.WriteReconciliationFailed(result.Reconciliation);
            }

            _logger.WriteFinished();
            return result;
        }
    }
}
