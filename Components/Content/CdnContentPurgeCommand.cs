// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class CdnContentPurgeCommand
    {
        private readonly ExposureContentDbContext _ContentDbContext;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ICdnContentConfig _Config;

        public CdnContentPurgeCommand(ExposureContentDbContext contentDbContext, IUtcDateTimeProvider dateTimeProvider, ICdnContentConfig config)
        {
            _ContentDbContext = contentDbContext;
            _DateTimeProvider = dateTimeProvider;
            _Config = config;
        }

        public async Task Execute()
        {
            await DeleteContent<RiskCalculationContentEntity>(_Config.ContentLifetimeDays);
            await DeleteContent<ResourceBundleContentEntity>(_Config.ContentLifetimeDays);
            await DeleteContent<AppConfigContentEntity>(_Config.ContentLifetimeDays);
            await DeleteContent<ExposureKeySetContentEntity>(_Config.ExposureKeySetLifetimeDays);

            _ContentDbContext.SaveAndCommit();
        }

        private async Task DeleteContent<T>(double lifetime) where T : ContentEntity
        {
            var now = _DateTimeProvider.Now();

            var dbSet = _ContentDbContext.Set<T>();

            var exceptEntity = dbSet
                .Where(x => x.Release <= now)
                .OrderByDescending(x => x.Release)
                .FirstOrDefault();

            if (exceptEntity != null)
            {
                var id = exceptEntity.Id;

                var recordToDelete = dbSet
                    .Where(x => x.Release <= now.AddDays(lifetime))
                    .Where(x => x.Id != id)
                    .OrderByDescending(x => x.Release)
                    .ToList();

                await _ContentDbContext.BulkDeleteAsync(recordToDelete);
            }
        }
    }
}
