// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
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
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class CdnContentPurgeCommand
    {
        private readonly ExposureContentDbContext _ContentDbContext;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ICdnContentConfig _Config;
        private readonly ILogger _Logger;

        public CdnContentPurgeCommand(ExposureContentDbContext contentDbContext, IUtcDateTimeProvider dateTimeProvider, ICdnContentConfig config, ILogger<CdnContentPurgeCommand> logger)
        {
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute()
        {
            _Logger.LogInformation($"Cleanup start.");
            await DeleteContent<RiskCalculationContentEntity>(_Config.ContentLifetimeDays);
            await DeleteContent<ResourceBundleContentEntity>(_Config.ContentLifetimeDays);
            await DeleteContent<AppConfigContentEntity>(_Config.ContentLifetimeDays);
            await DeleteContent<ExposureKeySetContentEntity>(_Config.ExposureKeySetLifetimeDays);
            _ContentDbContext.SaveAndCommit();
            _Logger.LogInformation($"Cleanup complete.");
        }

        private async Task DeleteContent<T>(double lifetime) where T : ContentEntity
        {
            var now = _DateTimeProvider.Now();

            var dbSet = _ContentDbContext.Set<T>();

            //TODO review this and how it works for EKS??? Will this leave 1 EKS alive?
            var exceptEntity = dbSet
                .Where(x => x.Release <= now)
                .OrderByDescending(x => x.Release)
                .FirstOrDefault();

            _Logger.LogWarning($"No published entity - {typeof(T).Name}.");

            if (exceptEntity != null)
            {
                _Logger.LogDebug($"Current published entity - {typeof(T).Name}, Id:{exceptEntity.PublishingId}.");

                var id = exceptEntity.Id;

                var recordToDelete = dbSet
                    .Where(x => x.Release <= now.AddDays(lifetime))
                    .Where(x => x.Id != id)
                    .OrderByDescending(x => x.Release)
                    .ToList();

                foreach (var i in recordToDelete)
                {
                    _Logger.LogInformation($"Cleanup - {typeof(T).Name}, Id:{i.PublishingId}.");
                }

                await _ContentDbContext.BulkDeleteAsync(recordToDelete);
            }
        }
    }
}
