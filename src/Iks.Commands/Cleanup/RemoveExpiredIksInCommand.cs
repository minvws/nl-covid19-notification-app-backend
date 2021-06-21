// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup
{
    public class RemoveExpiredIksInCommand
    {
        private readonly Func<IksInDbContext> _dbContextProvider;
        private readonly RemoveExpiredIksLoggingExtensions _logger;

        private readonly IUtcDateTimeProvider _utcDateTimeProvider;
        private RemoveExpiredIksCommandResult _result;
        private readonly IIksCleaningConfig _iksCleaningConfig;

        public RemoveExpiredIksInCommand(Func<IksInDbContext> dbContext, RemoveExpiredIksLoggingExtensions logger, IUtcDateTimeProvider utcDateTimeProvider, IIksCleaningConfig iksCleaningConfig)
        {
            _dbContextProvider = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _utcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _iksCleaningConfig = iksCleaningConfig;
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public async Task<RemoveExpiredIksCommandResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _logger.WriteStart("IksIn");

            _result = new RemoveExpiredIksCommandResult();

            var lifetimeDays = _iksCleaningConfig.LifetimeDays;
            var cutoff = (_utcDateTimeProvider.Snapshot - TimeSpan.FromDays(lifetimeDays)).Date;

            using (var dbc = _dbContextProvider())
            {
                using (var tx = dbc.BeginTransaction())
                {
                    _result.Found = dbc.Received.Count();
                    _logger.WriteCurrentIksFound(_result.Found);

                    var zombies = dbc.Received
                        .Where(x => x.Created < cutoff)
                        .Select(x => new { x.Id, x.Created })
                        .ToList();

                    _result.Zombies = zombies.Count;

                    _logger.WriteTotalIksFound(cutoff, _result.Zombies);

                    // DELETE FROM IksIn.dbo.IksIn WHERE Created < (today - 14-days)                    
                    _result.GivenMercy = await dbc.Database.ExecuteSqlRawAsync($"DELETE FROM {TableNames.IksIn} WHERE [Created] < '{cutoff:yyyy-MM-dd HH:mm:ss.fff}';");
                    await tx.CommitAsync();

                    _result.Remaining = dbc.Received.Count();
                }
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
