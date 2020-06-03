// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Components
{
    public class CreateDatabaseAndCollections
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbContextProvider;
        private readonly IPublishingIdCreator _PublishingIdCreator;

        public CreateDatabaseAndCollections(IDbContextProvider<ExposureContentDbContext>contextProvider, IPublishingIdCreator publishingIdCreator)
        {
            _DbContextProvider = contextProvider;
            _PublishingIdCreator = publishingIdCreator;
        }

        public async Task Execute()
        {
            _DbContextProvider.Current.Database.EnsureCreated();
            using var tx = _DbContextProvider.Current.Database.BeginTransaction();

            var e0 = new MobileDeviceRivmAdviceArgs
            {
                Release = new DateTime(2020, 1, 1),
                Text = new[]
                {
                        new LocalizableTextArgs
                        {
                            Locale = "en-GB", IsolationAdviceShort = "1st", IsolationAdviceLong = "First",
                        },
                        new LocalizableTextArgs
                        {
                            Locale = "nl-nl", IsolationAdviceShort = "1e", IsolationAdviceLong = "Eerste",
                        }
                    }
            }.ToEntity();
            e0.PublishingId = _PublishingIdCreator.Create(e0);
            await _DbContextProvider.Current.AddAsync(e0);

            var e1 = new MobileDeviceRivmAdviceArgs
            {
                Release = new DateTime(2020, 5, 1),
                IsolationPeriodDays = 10,
                ObservedTemporaryExposureKeyRetentionDays = 14,
                TemporaryExposureKeyRetentionDays = 15,
                Text = new[]
                {
                        new LocalizableTextArgs
                        {
                            Locale = "en-GB", IsolationAdviceShort = "Stay the hell indoors for {0} day!!!", IsolationAdviceLong = "Something hmtl, zipped",
                        },
                        new LocalizableTextArgs
                        {
                            Locale = "nl-nl", IsolationAdviceShort = "Verklaar de hel binnenshuis", IsolationAdviceLong = "Verklaar de hel binnenshuis but longer.",
                        }
                    }
            }.ToEntity();
            e1.PublishingId = _PublishingIdCreator.Create(e1);
            await _DbContextProvider.Current.AddAsync(e1);

            var e2 = new MobileDeviceRivmAdviceArgs
            {
                Release = new DateTime(2021, 1, 1),
                Text = new LocalizableTextArgs[0]
            }.ToEntity();
            e2.PublishingId = _PublishingIdCreator.Create(e2);
            await _DbContextProvider.Current.AddAsync(e2);

            //TODO something more realistic
            var e4 = new RiskCalculationConfigArgs
            {
                Release = new DateTime(2020, 5, 1),
                MinimumRiskScore = 4,
                Attenuation = new WeightingArgs { Weight = 30, LevelValues = new[] { 1 } },
                DaysSinceLastExposure = new WeightingArgs { Weight = 40, LevelValues = new[] { 1, 2, 3, 4 } },
                DurationLevelValues = new WeightingArgs { Weight = 50, LevelValues = new[] { 10, 2, 3, 4, 5 } },
                TransmissionRisk = new WeightingArgs { Weight = 60, LevelValues = new[] { 10, 100, 1000 } },
            }.ToEntity();
            e4.PublishingId = _PublishingIdCreator.Create(e4);
            await _DbContextProvider.Current.AddAsync(e4);

            tx.Commit();
        }
    }
}
