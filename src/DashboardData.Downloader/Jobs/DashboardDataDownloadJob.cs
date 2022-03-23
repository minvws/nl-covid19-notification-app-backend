// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader.Jobs
{
    public class DashboardDataDownloadJob : IJob
    {
        private readonly DashboardDataDbContext _dbContext;
        private readonly IDashboardDataConfig _dashboardDataConfig;
        private readonly ISha256HashService _sha256HashService;
        private readonly ILogger _logger;

        public DashboardDataDownloadJob(
            DashboardDataDbContext dbContext,
            IDashboardDataConfig dashboardDataConfig,
            ISha256HashService sha256HashService,
            ILogger<DashboardDataDownloadJob> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dashboardDataConfig = dashboardDataConfig ?? throw new ArgumentNullException(nameof(dashboardDataConfig));
            _sha256HashService = sha256HashService ?? throw new ArgumentNullException(nameof(sha256HashService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async void Run()
        {
            using var httpClient = new HttpClient();
            try
            {
                var response = httpClient.GetAsync(_dashboardDataConfig.DashboardDataDownloadUrl).Result;

                var jsonString = await response.Content.ReadAsStringAsync();

                var hash = _sha256HashService.Create(Encoding.UTF8.GetBytes(jsonString));

                // Check if we have already downloaded this json
                if (_dbContext.DashboardInputJson.Any(x => x.Hash == hash))
                {
                    _logger.LogInformation("Dashboard json with hash {Hash} already downloaded", hash);
                    return;
                }

                //TODO: add check on last_generated

                var dashboardInputJsonEntity = new DashboardInputJsonEntity()
                {
                    DownloadedDate = DateTime.UtcNow,
                    Hash = hash,
                    JsonData = jsonString
                };

                _logger.LogInformation("Writing dashboard json with hash {Hash} to database", hash);

                await using (_dbContext.BeginTransaction())
                {
                    await _dbContext.DashboardInputJson.AddAsync(dashboardInputJsonEntity);
                    _dbContext.SaveAndCommit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading dashboard json");
            }
        }
    }
}
