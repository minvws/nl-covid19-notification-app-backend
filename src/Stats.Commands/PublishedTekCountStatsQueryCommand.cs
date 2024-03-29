// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands
{
    public class PublishedTekCountStatsQueryCommand : IStatsQueryCommand
    {
        private readonly WorkflowDbContext _dbContext;

        public PublishedTekCountStatsQueryCommand(WorkflowDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public const string Name = "TekPublishedCount";

        public async Task<StatisticArgs> ExecuteAsync()
        {
            return new StatisticArgs
            {
                Name = Name,
                Value = await _dbContext.TemporaryExposureKeys.CountAsync(x => x.PublishingState == PublishingState.Published)
            };
        }
    }
}
