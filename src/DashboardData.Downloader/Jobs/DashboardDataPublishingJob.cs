// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader.Jobs
{
    public class DashboardDataPublishingJob : IJob
    {
        private readonly ContentInsertDbCommand _insertDbCommand;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly DashboardDataDbContext _dbContext;
        private readonly ContentDbContext _contentDbContext;
        private readonly ContentValidator _validator;
        private readonly ILogger _logger;

        public DashboardDataPublishingJob(
            ContentInsertDbCommand insertDbCommand,
            IUtcDateTimeProvider dateTimeProvider,
            DashboardDataDbContext dbContext,
            ContentDbContext contentDbContext,
            ContentValidator validator,
            ILogger<DashboardDataPublishingJob> logger)
        {
            _insertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Run()
        {
            // Get latest output json data
            var outputJsonEntity = _dbContext.DashboardOutputJson
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefault();

            if (outputJsonEntity == null)
            {
                _logger.LogInformation("No output dashboard data found, stopping.");
                return;
            }

            var outputJson = outputJsonEntity.JsonData;

            if (string.IsNullOrEmpty(outputJson))
            {
                _logger.LogError("No output dashboard json string found, stopping.");
                return;
            }

            // Publish output to content db
            var contentArgs = new ContentArgs
            {
                Release = _dateTimeProvider.Snapshot,
                ContentType = ContentTypes.DashboardData,
                Json = outputJson
            };

            if (!_validator.IsValid(contentArgs))
            {
                throw new InvalidOperationException($"Content not valid, creation date of entity: {outputJsonEntity.CreatedDate}");
            }

            _logger.LogDebug("Writing {ContentType} to database.", contentArgs.ContentType);

            _contentDbContext.BeginTransaction();
            _insertDbCommand.ExecuteAsync(contentArgs).GetAwaiter().GetResult();
            _contentDbContext.SaveAndCommit();

            _logger.LogDebug("Done writing {ContentType} to database.", contentArgs.ContentType);
        }
    }
}
