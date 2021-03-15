// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class RemoveExpiredManifestsCommand
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly Func<ContentDbContext> _DbContextProvider;
        private readonly ExpiredManifestLoggingExtensions _Logger;
        private readonly IManifestConfig _ManifestConfig;
        private RemoveExpiredManifestsCommandResult? _Result;

        public RemoveExpiredManifestsCommand(Func<ContentDbContext> dbContextProvider, ExpiredManifestLoggingExtensions logger, IManifestConfig manifestConfig, IUtcDateTimeProvider dateTimeProvider)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ManifestConfig = manifestConfig ?? throw new ArgumentNullException(nameof(manifestConfig));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(manifestConfig));
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public async Task<RemoveExpiredManifestsCommandResult> ExecuteAsync()
        {
            if (_Result != null)
                throw new InvalidOperationException("Object already used.");

            _Result = new RemoveExpiredManifestsCommandResult();

            _Logger.WriteStart(_ManifestConfig.KeepAliveCount);

            await using (var dbContext = _DbContextProvider())
            await using (var tx = dbContext.BeginTransaction())
            {
                _Result.Found = dbContext.Content.Count();

                var zombies = dbContext.Content
                    .Where(x => x.Type == ContentTypes.Manifest && x.Release < _DateTimeProvider.Snapshot)
                    .OrderByDescending(x => x.Release)
                    .Skip(_ManifestConfig.KeepAliveCount)
                    .ToList();

                _Result.Zombies = zombies.Count;
                _Logger.WriteRemovingManifests(zombies.Count);
                foreach (var i in zombies)
                    _Logger.WriteRemovingEntry(i.PublishingId, i.Release);

                if (zombies.Count == 0)
                {
                    _Logger.WriteFinishedNothingRemoved();
                    return _Result;
                }

                dbContext.RemoveRange(zombies);
                _Result.GivenMercy = dbContext.SaveChanges();

                var futureZombies = dbContext.Content
                    .Where(x => x.Type == ContentTypes.Manifest && x.Release > _DateTimeProvider.Snapshot)
                    .ToList();

                dbContext.RemoveRange(futureZombies);
                _Result.GivenMercy += dbContext.SaveChanges();

                _Result.Remaining = dbContext.Content.Count();

                tx.Commit();
            }

            _Logger.WriteFinished(_Result.Zombies, _Result.GivenMercy);

            if (_Result.Reconciliation != 0)
                _Logger.WriteReconcilliationFailed(_Result.Reconciliation);

            if (_Result.DeletionReconciliation != 0)
                _Logger.WriteDeletionReconciliationFailed(_Result.DeletionReconciliation);

            return _Result;
        }
    }
}