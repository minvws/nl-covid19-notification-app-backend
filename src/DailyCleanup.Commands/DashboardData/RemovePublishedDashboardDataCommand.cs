// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.DashboardData
{
    public class RemovePublishedDashboardDataCommand : BaseCommand
    {
        private RemovePublishedDashboardDataResult _result;
        private readonly DashboardDataDbContext _dbContext;

        public RemovePublishedDashboardDataCommand(DashboardDataDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _result = new RemovePublishedDashboardDataResult();

            var cleanupCandidates = await _dbContext.DashboardInputJson.AsNoTracking().Where(x => x.ProcessedDate != null).ToArrayAsync();

            return _result;
        }
    }
}
