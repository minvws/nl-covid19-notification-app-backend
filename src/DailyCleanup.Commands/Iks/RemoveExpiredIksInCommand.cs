// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Iks
{
    public class RemoveExpiredIksInCommand : BaseCommand
    {
        private readonly IksInDbContext _iksInDbContext;
        private readonly ILogger _logger;

        private readonly IUtcDateTimeProvider _utcDateTimeProvider;
        private RemoveExpiredIksCommandResult _result;
        private readonly IIksCleaningConfig _iksCleaningConfig;

        public RemoveExpiredIksInCommand(IksInDbContext iksInDbContext, ILogger<RemoveExpiredIksInCommand> logger, IUtcDateTimeProvider utcDateTimeProvider, IIksCleaningConfig iksCleaningConfig)
        {
            _iksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _utcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _iksCleaningConfig = iksCleaningConfig;
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public override async Task<ICommandResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _logger.LogInformation("Begin removing expired IksIn.");

            _result = new RemoveExpiredIksCommandResult();

            var lifetimeDays = _iksCleaningConfig.LifetimeDays;
            var cutoff = (_utcDateTimeProvider.Snapshot - TimeSpan.FromDays(lifetimeDays)).Date;

            _result.Found = _iksInDbContext.Received.Count();
            _logger.LogInformation("Current IksIn - Count: {CurrentIksInFound}.", _result.Found);

            var zombies = _iksInDbContext.Received
                .Where(x => x.Created < cutoff)
                .Select(x => new { x.Id, x.Created })
                .ToList();

            _result.Zombies = zombies.Count;
            _logger.LogInformation("Found expired IksIn - Cutoff: {IksInCutoff:yyyy-MM-dd}, Count: {TotalIksInFound}", cutoff, _result.Zombies);

            // DELETE FROM IksIn.dbo.IksIn WHERE Created < (today - 14-days)
            var iksToBeCleaned = await _iksInDbContext.Received.AsNoTracking().Where(p => p.Created < cutoff).ToArrayAsync();
            _result.GivenMercy = iksToBeCleaned.Length;

            var idsToDelete = string.Join(",", iksToBeCleaned.Select(x => x.Id.ToString()).ToArray());
            await _iksInDbContext.BulkDeleteSqlRawAsync<IksInEntity>(idsToDelete);

            _result.Remaining = _iksInDbContext.Received.Count();
            _logger.LogInformation("Removed expired IksIn - Count: {IksInRemoved}, Remaining: {IksInRemaining}", _result.GivenMercy, _result.Remaining);

            if (_result.Reconciliation != 0)
            {
                _logger.LogError("Reconciliation failed - Found-GivenMercy-Remaining: {IksInReconciliationCount}.", _result.Reconciliation);
            }

            _logger.LogInformation("Finished IksIn cleanup.");
            return _result;
        }
    }
}
