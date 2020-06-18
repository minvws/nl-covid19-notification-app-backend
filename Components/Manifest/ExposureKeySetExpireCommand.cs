// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
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

        public ExposureKeySetExpireCommand(ExposureContentDbContext config, IUtcDateTimeProvider utcDateTimeProvider, IGaenContentConfig gaenContentConfig)
        {
            _DbConfig = config;
            _UtcDateTimeProvider = utcDateTimeProvider;
            _GaenContentConfig = gaenContentConfig;
        }

        public async Task Execute()
        {
            var now = _UtcDateTimeProvider.Now();
            
            var timeToDie = _DbConfig.Set<ExposureKeySetContentEntity>()
                .Where(x => x.Release < now - TimeSpan.FromDays(_GaenContentConfig.ExposureKeySetLifetimeDays))
                .ToList();

            await _DbConfig.BulkDeleteAsync(timeToDie);
        }
    }
}