// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Eks
{
    public class RemoveExpiredEksV3Command : BaseCommand
    {
        private readonly ContentDbContext _dbContext;
        private readonly IEksConfig _config;
        private readonly IUtcDateTimeProvider _dtp;
        private readonly ILogger _logger;

        public RemoveExpiredEksV3Command(ContentDbContext dbContext, IEksConfig config, IUtcDateTimeProvider dtp, ILogger<RemoveExpiredEksV3Command> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            var result = new RemoveExpiredEksCommandResult();

            _logger.LogInformation("Begin removing expired EKSv3.");

            var cutoff = (_dtp.Snapshot - TimeSpan.FromDays(_config.LifetimeDays)).Date;

            result.Found = _dbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV3);
            _logger.LogInformation("Current EKS - Count: {CurrentEksV3Found}.", result.Found);

            var zombies = _dbContext.Content
                .Where(x => x.Type == ContentTypes.ExposureKeySetV3 && x.Release < cutoff)
                .Select(x => new { x.PublishingId, x.Release })
                .ToList();

            result.Zombies = zombies.Count;
            _logger.LogInformation("Found expired EKSv3 - Cutoff: {EksV3Cutoff:yyyy-MM-dd}, Count: {TotalEksV3Found}", cutoff, result.Zombies);

            foreach (var i in zombies)
            {
                _logger.LogInformation("Found expired EKSv3 - PublishingId: {EksV3PublishingId} Release: {EksV3Release}", i.PublishingId, i.Release);
            }

            if (!_config.CleanupDeletesData)
            {
                _logger.LogInformation("Finished EKSv3 cleanup. In safe mode - no deletions.");
                result.Remaining = result.Found;
                return result;
            }

            var eksToBeCleaned = await _dbContext.Content.AsNoTracking().Where(p => p.Type == ContentTypes.ExposureKeySetV3 && p.Release < cutoff).ToArrayAsync();
            result.GivenMercy = eksToBeCleaned.Length;
            await _dbContext.BulkDeleteWithTransactionAsync(eksToBeCleaned, new SubsetBulkArgs());
            result.Remaining = _dbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV3);

            _logger.LogInformation("Removed expired EKSv3 - Count: {EksV3Count}, Remaining: {EksV3Remaining}", result.GivenMercy, result.Remaining);

            if (result.Reconciliation != 0)
            {
                _logger.LogError("Reconciliation failed - Found-GivenMercy-Remaining: {EksV3Remaining}.", result.Reconciliation);
            }

            _logger.LogInformation("Finished EKSv3 cleanup.");
            return result;
        }
    }
}
