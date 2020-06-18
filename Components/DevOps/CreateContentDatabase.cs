// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class CreateContentDatabase
    {
        private readonly ExposureContentDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly StandardContentEntityFormatter _Formatter;

        public CreateContentDatabase(IConfiguration configuration, IUtcDateTimeProvider dateTimeProvider, ContentSigner signer)
        {
            var config = new StandardEfDbConfig(configuration, "Content");
            var builder = new SqlServerDbContextOptionsBuilder(config);
            _DbContextProvider = new ExposureContentDbContext(builder.Build());
            _DateTimeProvider = dateTimeProvider;
            _Formatter = new StandardContentEntityFormatter(new ZippedSignedContentFormatter(signer), new StandardPublishingIdFormatter(signer));
        }

        public async Task Execute()
        {
            await _DbContextProvider.Database.EnsureDeletedAsync();
            await _DbContextProvider.Database.EnsureCreatedAsync();
        }

        public async Task AddExampleContent()
        {
            await using var tx = await _DbContextProvider.Database.BeginTransactionAsync();

            await Write(
                new ResourceBundleArgs
                {
                    Release = _DateTimeProvider.Now(),
                    Text = new Dictionary<string, Dictionary<string, string>>
                    {
                        {
                            "en-GB", new Dictionary<string, string>()
                            {
                                {"InfectedMessage", "You're possibly infected"}
                            }
                        },
                        {
                            "nl-NL", new Dictionary<string, string>
                            {
                                {"InfectedMessage", "U bent mogelijk geinvecteerd"}
                            }
                        }
                    }
                }
            );

            await Write(
                new ResourceBundleArgs
                {
                    Release = _DateTimeProvider.Now(),
                    IsolationPeriodDays = 10,
                    ObservedTemporaryExposureKeyRetentionDays = 14,
                    TemporaryExposureKeyRetentionDays = 15,
                    Text = new Dictionary<string, Dictionary<string, string>>()
                    {
                        {
                            "en-GB", new Dictionary<string, string>
                            {
                                {"FirstLong", "First"},
                                {"FirstShort", "1st"}
                            }
                        },
                        {
                            "nl-NL", new Dictionary<string, string>
                            {
                                {"FirstLong", "Eerste"},
                                {"FirstShort", "1ste"}
                            }
                        }
                    }
                });

            await Write(
                new ResourceBundleArgs
                {
                    Release = _DateTimeProvider.Now()
                }
            );

            await Write(
                new RiskCalculationConfigArgs
                {
                    Release = new DateTime(2020, 6, 12),
                    MinimumRiskScore = 1,
                    DaysSinceLastExposureScores​ = new[] {1, 2, 3, 4, 5, 6, 7, 8},
                    AttenuationScores​ = new[] {1, 2, 3, 4, 5, 6, 7, 8},
                    DurationAtAttenuationThresholds​ = new[] {42, 56},
                    DurationScores = new[] {1, 2, 3, 4, 5, 6, 7, 8},
                    TransmissionRiskScores​ = new[] {1, 2, 3, 4, 5, 6, 7, 8},
                });

            await Write(
            new AppConfigArgs
            {
                Release = _DateTimeProvider.Now(),
                ManifestFrequency = 5,
                DecoyProbability = 1,
                Version = 123345
            });

            _DbContextProvider.SaveAndCommit();
        }

        private async Task Write(RiskCalculationConfigArgs a4)
        {
            var e4 = new RiskCalculationContentEntity
            {
                Release = a4.Release
            };
            await _Formatter.Fill(e4, a4.ToContent());
            await _DbContextProvider.AddAsync(e4);
        }
        private async Task Write(ResourceBundleArgs a4)
        {
            var e4 = new ResourceBundleContentEntity
            {
                Release = a4.Release
            };
            await _Formatter.Fill(e4, a4.ToContent());
            await _DbContextProvider.AddAsync(e4);
        }
        private async Task Write(AppConfigArgs a4)
        {
            var e4 = new AppConfigContentEntity
            {
                Release = a4.Release
            };
            await _Formatter.Fill(e4, a4.ToContent());
            await _DbContextProvider.AddAsync(e4);
        }
    }
}
