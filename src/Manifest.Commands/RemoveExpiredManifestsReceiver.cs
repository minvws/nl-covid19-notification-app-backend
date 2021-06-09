// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class RemoveExpiredManifestsReceiver
    {
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly Func<ContentDbContext> _dbContextProvider;
        private readonly IManifestConfig _manifestConfig;

        public RemoveExpiredManifestsReceiver(Func<ContentDbContext> dbContextProvider, IManifestConfig manifestConfig, IUtcDateTimeProvider dateTimeProvider)
        {
            _dbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _manifestConfig = manifestConfig ?? throw new ArgumentNullException(nameof(manifestConfig));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(manifestConfig));
        }

        public async Task<RemoveExpiredManifestsCommandResult> RemoveManifests(string manifestType, IExpiredManifestLogging logger)
        {
            var result = new RemoveExpiredManifestsCommandResult();

            if (result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            result = new RemoveExpiredManifestsCommandResult();

            logger.WriteStart(_manifestConfig.KeepAliveCount);

            await using (var dbContext = _dbContextProvider())
            await using (var tx = dbContext.BeginTransaction())
            {
                result.Found = dbContext.Content.Count();

                var zombies = dbContext.Content
                    .Where(x => x.Type == manifestType && x.Release < _dateTimeProvider.Snapshot)
                    .OrderByDescending(x => x.Release)
                    .Skip(_manifestConfig.KeepAliveCount)
                    .ToList();

                result.Zombies = zombies.Count;
                logger.WriteRemovingManifests(zombies.Count);
                foreach (var i in zombies)
                {
                    logger.WriteRemovingEntry(i.PublishingId, i.Release);
                }

                if (zombies.Count == 0)
                {
                    logger.WriteFinishedNothingRemoved();
                    return result;
                }

                dbContext.RemoveRange(zombies);
                result.GivenMercy = dbContext.SaveChanges();

                var futureZombies = dbContext.Content
                    .Where(x => x.Type == manifestType && x.Release > _dateTimeProvider.Snapshot)
                    .ToList();

                dbContext.RemoveRange(futureZombies);
                result.GivenMercy += dbContext.SaveChanges();

                result.Remaining = dbContext.Content.Count();

                tx.Commit();
            }

            logger.WriteFinished(result.Zombies, result.GivenMercy);

            if (result.Reconciliation != 0)
            {
                logger.WriteReconciliationFailed(result.Reconciliation);
            }

            if (result.DeletionReconciliation != 0)
            {
                logger.WriteDeletionReconciliationFailed(result.DeletionReconciliation);
            }

            return result;
        }
    }
}
