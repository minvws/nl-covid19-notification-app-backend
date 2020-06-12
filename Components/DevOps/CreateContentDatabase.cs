// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class CreateContentDatabase
    {
        private readonly ExposureContentDbContext _DbContextProvider;
        private readonly IPublishingId _PublishingId;

        public CreateContentDatabase(IConfiguration configuration, IPublishingId publishingId)
        {
            var config = new StandardEfDbConfig(configuration, "Content");
            var builder = new SqlServerDbContextOptionsBuilder(config);
            _DbContextProvider = new ExposureContentDbContext(builder.Build());
            _PublishingId = publishingId;
        }

        public async Task Execute()
        {
            await _DbContextProvider.Database.EnsureDeletedAsync();
            await _DbContextProvider.Database.EnsureCreatedAsync();
        }

        public async Task AddExampleContent()
        {
            await using var tx = await _DbContextProvider.Database.BeginTransactionAsync();

            var e0 = new ResourceBundleArgs
            {
                Release = DateTime.Now,
                Text = new Dictionary<string, Dictionary<string, string>>
                {
                    {"en-GB", new Dictionary<string, string>()
                    {
                        {"InfectedMessage","You're possibly infected"}
                    }},
                    {"nl-NL",new Dictionary<string, string>
                    {
                        {"InfectedMessage","U bent mogelijk geinvecteerd"}
                    }}
                }
            }.ToEntity();
            e0.PublishingId = _PublishingId.Create(e0);
            await _DbContextProvider.AddAsync(e0);

            var e1 = new ResourceBundleArgs
            {
                Release = DateTime.Now,
                IsolationPeriodDays = 10,
                ObservedTemporaryExposureKeyRetentionDays = 14,
                TemporaryExposureKeyRetentionDays = 15,
                Text = new Dictionary<string, Dictionary<string, string>>()
                {
                    {"en-GB", new Dictionary<string, string>
                    {
                        {"FirstLong","First"},
                        {"FirstShort","1st"}
                    }},
                    {"nl-NL", new Dictionary<string, string>
                    {
                        {"FirstLong","Eerste"},
                        {"FirstShort","1ste"}
                    }}
                }
            }.ToEntity();
            e1.PublishingId = _PublishingId.Create(e1);
            await _DbContextProvider.AddAsync(e1);

            var e2 = new ResourceBundleArgs
            {
                Release = DateTime.Now
            }.ToEntity();
            e2.PublishingId = _PublishingId.Create(e2);
            await _DbContextProvider.AddAsync(e2);

            var e4 = new RiskCalculationConfigArgs
            {
                Release = new DateTime(2020, 6, 12),
                MinimumRiskScore = 1,
                DaysSinceLastExposureScores​ = new[]{1,2,3,4,5,6,7,8},
                AttenuationScores​ = new[] { 1,2,3,4,5,6,7,8 },
                DurationAtAttenuationThresholds​ = new[] { 42,56 },
                DurationScores = new[] { 1,2,3,4,5,6,7,8 },
                TransmissionRiskScores​ = new[] { 1,2,3,4,5,6,7,8 },
            }.ToEntity();
            e4.PublishingId = _PublishingId.Create(e4);

            await _DbContextProvider.AddAsync(e4);
            await _DbContextProvider.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
