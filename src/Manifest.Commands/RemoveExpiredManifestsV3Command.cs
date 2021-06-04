// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class RemoveExpiredManifestsV3Command
    {
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly Func<ContentDbContext> _dbContextProvider;
        private readonly ExpiredManifestV3LoggingExtensions _logger;
        private readonly IManifestConfig _manifestConfig;
        private RemoveExpiredManifestsCommandResult _result;

        public RemoveExpiredManifestsV3Command(Func<ContentDbContext> dbContextProvider, ExpiredManifestV3LoggingExtensions logger, IManifestConfig manifestConfig, IUtcDateTimeProvider dateTimeProvider)
        {
            _dbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _manifestConfig = manifestConfig ?? throw new ArgumentNullException(nameof(manifestConfig));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(manifestConfig));
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public async Task<RemoveExpiredManifestsCommandResult> ExecuteAsync()
        {
            if (_result != null)
                throw new InvalidOperationException("Object already used.");

            _result = new RemoveExpiredManifestsCommandResult();

            _logger.WriteStart(_manifestConfig.KeepAliveCount);

            await using (var dbContext = _dbContextProvider())
            await using (var tx = dbContext.BeginTransaction())
            {
                _result.Found = dbContext.Content.Count();

                var zombies = dbContext.Content
                    .Where(x => x.Type == ContentTypes.ManifestV3 && x.Release < _dateTimeProvider.Snapshot)
                    .OrderByDescending(x => x.Release)
                    .Skip(_manifestConfig.KeepAliveCount)
                    .ToList();

                _result.Zombies = zombies.Count;
                _logger.WriteRemovingManifests(zombies.Count);
                foreach (var i in zombies)
                    _logger.WriteRemovingEntry(i.PublishingId, i.Release);

                if (zombies.Count == 0)
                {
                    _logger.WriteFinishedNothingRemoved();
                    return _result;
                }

                dbContext.RemoveRange(zombies);
                _result.GivenMercy = dbContext.SaveChanges();

                var futureZombies = dbContext.Content
                    .Where(x => x.Type == ContentTypes.ManifestV3 && x.Release > _dateTimeProvider.Snapshot)
                    .ToList();

                dbContext.RemoveRange(futureZombies);
                _result.GivenMercy += dbContext.SaveChanges();

                _result.Remaining = dbContext.Content.Count();

                tx.Commit();
            }

            _logger.WriteFinished(_result.Zombies, _result.GivenMercy);

            if (_result.Reconciliation != 0)
                _logger.WriteReconciliationFailed(_result.Reconciliation);

            if (_result.DeletionReconciliation != 0)
                _logger.WriteDeletionReconciliationFailed(_result.DeletionReconciliation);

            return _result;
        }
    }
}
