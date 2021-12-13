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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Iks
{
    public class RemoveExpiredIksOutCommand : BaseCommand
    {
        private readonly IksOutDbContext _iksOutDbContext;
        private readonly ILogger _logger;

        private readonly IUtcDateTimeProvider _utcDateTimeProvider;
        private RemoveExpiredIksCommandResult _result;
        private readonly IIksCleaningConfig _iksCleaningConfig;

        public RemoveExpiredIksOutCommand(IksOutDbContext iksOutDbContext, ILogger<RemoveExpiredIksOutCommand> logger, IUtcDateTimeProvider utcDateTimeProvider, IIksCleaningConfig iksCleaningConfig)
        {
            _iksOutDbContext = iksOutDbContext ?? throw new ArgumentNullException(nameof(iksOutDbContext));
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

            _logger.LogInformation("Begin removing expired IksOut.");

            _result = new RemoveExpiredIksCommandResult();

            var lifetimeDays = _iksCleaningConfig.LifetimeDays;
            var cutoff = (_utcDateTimeProvider.Snapshot - TimeSpan.FromDays(lifetimeDays)).Date;

            _result.Found = _iksOutDbContext.Iks.Count();
            _logger.LogInformation(" Current IksOut - Count: {CurrentIksOutFound}.", _result.Found);


            var zombies = _iksOutDbContext.Iks.AsNoTracking()
                .Where(x => x.Created < cutoff)
                .Select(x => new { x.Id, x.Created })
                .ToList();

            _result.Zombies = zombies.Count;
            _logger.LogInformation("Found expired IksOut - Cutoff: {IksOutCutoff:yyyy-MM-dd}, Count: {TotalIksOutFound}", cutoff, _result.Zombies);

            // DELETE FROM IksIn.dbo.IksIn WHERE Created < (today - 14-days)
            var iksToBeCleaned = await _iksOutDbContext.Iks.AsNoTracking().Where(p => p.Created < cutoff).ToArrayAsync();
            _result.GivenMercy = iksToBeCleaned.Length;
            await _iksOutDbContext.BulkDeleteWithTransactionAsync(iksToBeCleaned, new SubsetBulkArgs());

            _result.Remaining = _iksOutDbContext.Iks.Count();
            _logger.LogInformation("Removed expired IksOut - Count: {IksOutRemoved}, Remaining: {IksOutRemaining}", _result.GivenMercy, _result.Remaining);

            if (_result.Reconciliation != 0)
            {
                _logger.LogError("Reconciliation failed - Found-GivenMercy-Remaining: {IksOutReconciliationCount}.", _result.Reconciliation);
            }

            _logger.LogInformation("Finished IksOut cleanup.");
            return _result;
        }
    }
}
