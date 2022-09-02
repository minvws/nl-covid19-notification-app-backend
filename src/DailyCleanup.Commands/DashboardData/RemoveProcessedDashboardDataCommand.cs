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
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.DashboardData
{
    public class RemoveProcessedDashboardDataCommand : BaseCommand
    {
        private RemoveProcessedDashboardDataResult _result;
        private readonly DashboardDataDbContext _dbContext;
        private readonly ILogger _logger;

        public RemoveProcessedDashboardDataCommand(DashboardDataDbContext dbContext, ILogger<RemoveProcessedDashboardDataCommand> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _result = new RemoveProcessedDashboardDataResult();

            var removalCandidates = await _dbContext.DashboardInputJson.AsNoTracking().Where(x => x.ProcessedDate != null).ToArrayAsync();

            if (removalCandidates.Any())
            {
                var idsToDelete = string.Join(",", removalCandidates.Select(x => x.Id.ToString()).ToArray());
                await _dbContext.BulkDeleteSqlRawAsync<DashboardInputJsonEntity>(idsToDelete);
            }


            _result.Removed = removalCandidates.Length;
            _logger.LogInformation("Removed {DashboardDataItemsRemoved} processed DashboardData jsons", _result.Removed);

            _result.Remaining = _dbContext.DashboardInputJson.AsNoTracking().Count();
            _logger.LogInformation("Unprocessed DashboardData jsons remaining: {DashboardDataItemsRemaining}", _result.Remaining);

            // Nothing is actually done with this return value,
            // but putting it here for consistency with other cleanup commands.
            return _result;
        }
    }
}
