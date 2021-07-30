// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class RemoveExpiredManifestsReceiver
    {
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ContentDbContext _contentDbContext;
        private readonly IManifestConfig _manifestConfig;
        private readonly ILogger<RemoveExpiredManifestsReceiver> _logger;

        // Logging codex digits specific to expired manifest removal;
        // added to the logging codex base number during logging.
        private const int Start = 0;
        private const int Finished = 99;
        private const int RemovingManifests = 1;
        private const int RemovingEntry = 2;
        private const int ReconciliationFailed = 3;
        private const int DeletionReconciliationFailed = 4;
        private const int FinishedNothingRemoved = 98;

        public RemoveExpiredManifestsReceiver(ContentDbContext contentDbContext, IManifestConfig manifestConfig, IUtcDateTimeProvider dateTimeProvider, ILogger<RemoveExpiredManifestsReceiver> logger)
        {
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _manifestConfig = manifestConfig ?? throw new ArgumentNullException(nameof(manifestConfig));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(manifestConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RemoveExpiredManifestsCommandResult> RemoveManifestsAsync(ContentTypes manifestType, int loggingBaseNumber)
        {
            var result = new RemoveExpiredManifestsCommandResult();

            _logger.LogInformation("[{Name}/{Id}] Begin removing expired Manifests - Keep Alive Count:{Count}.",
                manifestType, loggingBaseNumber + Start, _manifestConfig.KeepAliveCount);

            await using (var tx = _contentDbContext.BeginTransaction())
            {
                result.Found = _contentDbContext.Content.Count();

                var zombies = _contentDbContext.Content
                    .Where(x => x.Type == manifestType && x.Release < _dateTimeProvider.Snapshot)
                    .OrderByDescending(x => x.Release)
                    .Skip(_manifestConfig.KeepAliveCount)
                    .ToList();

                result.Zombies = zombies.Count;

                _logger.LogInformation("[{Name}/{Id}] Removing expired Manifests - Count:{Count}.", manifestType,
                    loggingBaseNumber + RemovingManifests, zombies.Count);

                foreach (var i in zombies)
                {
                    _logger.LogInformation("[{Name}/{Id}] Removing expired Manifest - PublishingId:{PublishingId} Release:{Release}.",
                        manifestType, loggingBaseNumber + RemovingEntry, i.PublishingId, i.Release);
                }

                if (zombies.Count == 0)
                {
                    _logger.LogInformation("[{Name}/{Id}] Finished removing expired Manifests - Nothing to remove.",
                        manifestType, loggingBaseNumber + FinishedNothingRemoved);

                    return result;
                }

                _contentDbContext.RemoveRange(zombies);
                result.GivenMercy = _contentDbContext.SaveChanges();

                var futureZombies = _contentDbContext.Content
                    .Where(x => x.Type == manifestType && x.Release > _dateTimeProvider.Snapshot)
                    .ToList();

                _contentDbContext.RemoveRange(futureZombies);
                result.GivenMercy += _contentDbContext.SaveChanges();

                result.Remaining = _contentDbContext.Content.Count();

                await tx.CommitAsync();
            }

            _logger.LogInformation("[{Name}/{Id}] Finished removing expired Manifests - ExpectedCount:{count} ActualCount:{givenMercy}.",
                manifestType, loggingBaseNumber + Finished, result.Zombies, result.GivenMercy);

            if (result.Reconciliation != 0)
            {
                _logger.LogError("[{Name}/{Id}] Reconciliation failed removing expired Manifests - Found-GivenMercy-Remaining={reconciliation}.",
                    manifestType, loggingBaseNumber + ReconciliationFailed, result.Reconciliation);
            }

            if (result.DeletionReconciliation != 0)
            {
                _logger.LogError("[{Name}/{Id}] Reconciliation failed removing expired Manifests - Zombies-GivenMercy={deadReconciliation}.",
                    manifestType, loggingBaseNumber + DeletionReconciliationFailed, result.DeletionReconciliation);
            }

            return result;
        }
    }
}
