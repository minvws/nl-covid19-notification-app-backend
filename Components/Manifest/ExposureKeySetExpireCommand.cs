// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ExposureKeySetExpireCommand
    {
        private readonly ExposureContentDbContext _DbConfig;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly IGaenContentConfig _GaenContentConfig;
        private readonly ILogger _Logger;

        public ExposureKeySetExpireCommand(ExposureContentDbContext dbConfig, IUtcDateTimeProvider utcDateTimeProvider, IGaenContentConfig gaenContentConfig, ILogger logger)
        {
            _DbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
            _UtcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _GaenContentConfig = gaenContentConfig ?? throw new ArgumentNullException(nameof(gaenContentConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute()
        {
            var now = _UtcDateTimeProvider.Now();
            
            var timeToDie = _DbConfig.Set<ExposureKeySetContentEntity>()
                .Where(x => x.Release < now - TimeSpan.FromDays(_GaenContentConfig.ExposureKeySetLifetimeDays))
                .ToList();

            foreach (var i in timeToDie)
            {
                _Logger.LogInformation($"Deleting EKS - {i.PublishingId}.");
            }

            await _DbConfig.BulkDeleteAsync(timeToDie);
        }
    }
}