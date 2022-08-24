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

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Manifest
{
    public class RemoveExpiredManifestsCommand : BaseCommand
    {
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ContentDbContext _contentDbContext;
        private readonly IManifestConfig _manifestConfig;
        private readonly ILogger _logger;

        public RemoveExpiredManifestsCommand(
            ContentDbContext contentDbContext,
            IManifestConfig manifestConfig,
            IUtcDateTimeProvider dateTimeProvider,
            ILogger<RemoveExpiredManifestsCommand> logger)
        {
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _manifestConfig = manifestConfig ?? throw new ArgumentNullException(nameof(manifestConfig));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            var result = new RemoveExpiredManifestsCommandResult();

            _logger.LogInformation("Begin removing expired {ManifestType} - Keep Alive Count: {Count}",
                ContentTypes.Manifest, _manifestConfig.KeepAliveCount);

            await using (var tx = _contentDbContext.BeginTransaction())
            {
                result.Found = _contentDbContext.Content.Count();

                var zombies = _contentDbContext.Content
                    .Where(x => x.Type == ContentTypes.Manifest && x.Release < _dateTimeProvider.Snapshot)
                    .OrderByDescending(x => x.Release)
                    .Skip(_manifestConfig.KeepAliveCount)
                    .ToList();

                result.Zombies = zombies.Count;

                _logger.LogInformation("Removing expired {ManifestType} - Count: {Count}", ContentTypes.Manifest, zombies.Count);

                foreach (var i in zombies)
                {
                    _logger.LogInformation("Removing expired {ManifestType} - PublishingId: {PublishingId} Release: {Release}",
                        ContentTypes.Manifest, i.PublishingId, i.Release);
                }

                if (zombies.Count == 0)
                {
                    _logger.LogInformation("Finished removing expired {ManifestType} - Nothing to remove", ContentTypes.Manifest);

                    return result;
                }

                _contentDbContext.RemoveRange(zombies);
                result.GivenMercy = await _contentDbContext.SaveChangesAsync();

                var futureZombies = _contentDbContext.Content
                    .Where(x => x.Type == ContentTypes.Manifest && x.Release > _dateTimeProvider.Snapshot)
                    .ToList();

                _contentDbContext.RemoveRange(futureZombies);
                result.GivenMercy += await _contentDbContext.SaveChangesAsync();

                result.Remaining = _contentDbContext.Content.Count();

                await tx.CommitAsync();
            }

            _logger.LogInformation("Finished removing expired {ManifestType} - ExpectedCount: {ManiestCount} ActualCount: {ManifestGivenMercy}.",
                ContentTypes.Manifest, result.Zombies, result.GivenMercy);

            if (result.Reconciliation != 0)
            {
                _logger.LogError("Reconciliation failed removing expired {ManifestType} - Found-GivenMercy-Remaining = {ManifestReconciliation}.",
                    ContentTypes.Manifest, result.Reconciliation);
            }

            if (result.DeletionReconciliation != 0)
            {
                _logger.LogError("Reconciliation failed removing expired {ManifestType} - Zombies-GivenMercy = {ManifestDeadReconciliation}.",
                    ContentTypes.Manifest, result.DeletionReconciliation);
            }

            return result;
        }
    }
}
