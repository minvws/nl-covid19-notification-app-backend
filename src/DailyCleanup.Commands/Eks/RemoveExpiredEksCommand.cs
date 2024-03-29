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
    public class RemoveExpiredEksCommand : BaseCommand
    {
        private readonly ContentDbContext _dbContext;
        private readonly IEksConfig _config;
        private readonly IUtcDateTimeProvider _dtp;
        private readonly ILogger _logger;

        public RemoveExpiredEksCommand(ContentDbContext dbContext, IEksConfig config, IUtcDateTimeProvider dtp, ILogger<RemoveExpiredEksCommand> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            var result = new RemoveExpiredEksCommandResult();

            _logger.LogInformation("Begin removing expired EKS");

            var cutoff = (_dtp.Snapshot - TimeSpan.FromDays(_config.LifetimeDays)).Date;

            result.Found = _dbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet);
            _logger.LogInformation("Current EKS - Count: {CurrentEksFound}", result.Found);

            var zombies = _dbContext.Content
                .Where(x => x.Type == ContentTypes.ExposureKeySet && x.Release < cutoff)
                .Select(x => new { x.PublishingId, x.Release })
                .ToList();

            result.Zombies = zombies.Count;
            _logger.LogInformation("Found expired EKS - Cutoff: {EksCutoff:yyyy-MM-dd}, Count: {TotalEksFound}", cutoff, result.Zombies);

            foreach (var i in zombies)
            {
                _logger.LogInformation("Found expired EKS - PublishingId: {ContentId} Release: {EksRelease}", i.PublishingId, i.Release);
            }

            if (!_config.CleanupDeletesData)
            {
                _logger.LogInformation("Finished EKS cleanup. In safe mode - no deletions");
                result.Remaining = result.Found;
                return result;
            }

            var eksToBeCleaned = await _dbContext.Content.AsNoTracking().Where(p => p.Type == ContentTypes.ExposureKeySet && p.Release < cutoff).ToArrayAsync();
            result.GivenMercy = eksToBeCleaned.Length;

            if (eksToBeCleaned.Any())
            {
                var idsToDelete = string.Join(",", eksToBeCleaned.Select(x => x.Id.ToString()).ToArray());
                await _dbContext.BulkDeleteSqlRawAsync<ContentEntity>(idsToDelete);
            }

            result.Remaining = _dbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet);

            _logger.LogInformation("Removed expired EKS - Count: {EksCount}, Remaining: {EksRemaining}", result.GivenMercy, result.Remaining);

            if (result.Reconciliation != 0)
            {
                _logger.LogError("Reconciliation failed - Found-GivenMercy-Remaining: {EksRemaining}", result.Reconciliation);
            }

            _logger.LogInformation("Finished EKS cleanup");
            return result;
        }
    }
}
