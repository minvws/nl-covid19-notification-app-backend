// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader.Jobs
{
    public class DashboardDataProcessJob : IJob
    {
        private readonly DashboardDataDbContext _dbContext;
        private readonly IDashboardDataConfig _dashboardDataConfig;
        private readonly ISha256HashService _sha256HashService;
        private readonly ILogger _logger;

        public DashboardDataProcessJob(
            DashboardDataDbContext dbContext,
            IDashboardDataConfig dashboardDataConfig,
            ISha256HashService sha256HashService,
            ILogger<DashboardDataProcessJob> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dashboardDataConfig = dashboardDataConfig ?? throw new ArgumentNullException(nameof(dashboardDataConfig));
            _sha256HashService = sha256HashService ?? throw new ArgumentNullException(nameof(sha256HashService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async void Run()
        {
            try
            {
                // Get latest downloaded json data
                var dashboardInputJsonEntity = _dbContext.DashboardInputJson
                    .Where(x => x.ProcessedDate == null)
                    .OrderByDescending(x => x.DownloadedDate)
                    .FirstOrDefault();

                if (dashboardInputJsonEntity == null)
                {
                    _logger.LogInformation("No unprocessed downloaded dashboard data found, stopping.");
                    return;
                }

                var jsonString = dashboardInputJsonEntity.JsonData;

                if (string.IsNullOrEmpty(jsonString))
                {
                    _logger.LogError("No downloaded dashboard json string found, stopping.");
                    return;
                }

                _logger.LogInformation("Retrieved downloaded json.");

                // Transform downloaded json data into our output model
                var dashboardInputModel = CreateInputModel(jsonString);
                var dashboardDataOutputModel = CreateOutputModel(dashboardInputModel);

                _logger.LogInformation("Serializing into output json.");

                var outputJson = JsonSerializer.Serialize(dashboardDataOutputModel, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var dashboardOutputJsonEntity = new DashboardOutputJsonEntity
                {
                    CreatedDate = DateTime.UtcNow,
                    JsonData = outputJson
                };

                // Write output json to db
                _logger.LogInformation("Writing output json to database");

                await using (_dbContext.BeginTransaction())
                {
                    await _dbContext.DashboardOutputJson.AddAsync(dashboardOutputJsonEntity);
                    _dbContext.SaveAndCommit();
                }

                // Mark downloaded json as processed
                var hash = _sha256HashService.Create(Encoding.UTF8.GetBytes(jsonString));

                _logger.LogInformation("Marking dashboard json with has {Hash} as processed", hash);

                await using (_dbContext.BeginTransaction())
                {
                    dashboardInputJsonEntity.ProcessedDate = DateTime.UtcNow;
                    _dbContext.SaveAndCommit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing dashboard json");
            }
        }

        private DashboardInputModel CreateInputModel(string jsonString)
        {
            _logger.LogInformation("Creating input model.");

            var result = JsonSerializer.Deserialize<DashboardInputModel>(jsonString);

            // Add the active user data from CDN
            var cdnStatsEntity = _dbContext.CdnStats.Select(x => x);
            result.CoronaMelderUsersInput = new CoronaMelderUsersInput
            {
                Values = cdnStatsEntity.Select(x => new CoronaMelderUsersInputValue
                {
                    AverageDailyUsers = x.AverageDailyUsers,
                    FirstDate = x.FirstDate,
                    LastDate = x.LastDate
                }).ToList()
            };

            return result;
        }

        private DashboardOutputModel CreateOutputModel(DashboardInputModel inputModel)
        {
            _logger.LogInformation("Creating output model.");

            var result = DashboardDataMapper.Map(inputModel, _dashboardDataConfig.CutOffInDays);

            // Add links to Coronadashboard pages
            result.MoreInfoUrl = _dashboardDataConfig.DashboardOverviewExternalLink;
            result.PositiveTestResults.MoreInfoUrl = _dashboardDataConfig.PositiveTestResultsExternalLink;
            result.HospitalAdmissions.MoreInfoUrl = _dashboardDataConfig.HospitalAdmissionsExternalLink;
            result.IcuAdmissions.MoreInfoUrl = _dashboardDataConfig.IcuAdmissionsExternalLink;
            result.VaccinationCoverage.MoreInfoUrl = _dashboardDataConfig.VaccinationCoverageExternalLink;
            result.CoronaMelderUsers.MoreInfoUrl = _dashboardDataConfig.CoronaMelderUsersExternalLink;

            return result;
        }
    }
}
