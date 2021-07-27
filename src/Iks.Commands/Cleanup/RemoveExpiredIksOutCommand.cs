// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup
{
    public class RemoveExpiredIksOutCommand : BaseCommand
    {
        private readonly IksOutDbContext _iksOutDbContext;
        private readonly RemoveExpiredIksLoggingExtensions _logger;

        private readonly IUtcDateTimeProvider _utcDateTimeProvider;
        private RemoveExpiredIksCommandResult _result;
        private readonly IIksCleaningConfig _iksCleaningConfig;

        public RemoveExpiredIksOutCommand(IksOutDbContext iksOutDbContext, RemoveExpiredIksLoggingExtensions logger, IUtcDateTimeProvider utcDateTimeProvider, IIksCleaningConfig iksCleaningConfig)
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

            _logger.WriteStart("IksOut");

            _result = new RemoveExpiredIksCommandResult();

            var lifetimeDays = _iksCleaningConfig.LifetimeDays;
            var cutoff = (_utcDateTimeProvider.Snapshot - TimeSpan.FromDays(lifetimeDays)).Date;

            await using (var tx = _iksOutDbContext.BeginTransaction())
            {
                _result.Found = _iksOutDbContext.Iks.Count();
                _logger.WriteCurrentIksFound(_result.Found);

                var zombies = _iksOutDbContext.Iks.AsNoTracking()
                    .Where(x => x.Created < cutoff)
                    .Select(x => new { x.Id, x.Created })
                    .ToList();

                _result.Zombies = zombies.Count;

                _logger.WriteTotalIksFound(cutoff, _result.Zombies);

                // DELETE FROM IksIn.dbo.IksIn WHERE Created < (today - 14-days)
                var iksToBeCleaned = await _iksOutDbContext.Iks.AsNoTracking().Where(p => p.Created < cutoff).ToArrayAsync();
                _result.GivenMercy = iksToBeCleaned.Length;
                await _iksOutDbContext.BulkDeleteAsync(iksToBeCleaned);
                await tx.CommitAsync();

                _result.Remaining = _iksOutDbContext.Iks.Count();
            }

            _logger.WriteRemovedAmount(_result.GivenMercy, _result.Remaining);

            if (_result.Reconciliation != 0)
            {
                _logger.WriteReconciliationFailed(_result.Reconciliation);
            }

            _logger.WriteFinished();
            return _result;
        }
    }
}
