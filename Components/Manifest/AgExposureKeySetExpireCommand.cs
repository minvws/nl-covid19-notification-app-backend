// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AgProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class AgExposureKeySetExpireCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbConfig;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly IAgConfig _AgConfig;

        public AgExposureKeySetExpireCommand(IDbContextProvider<ExposureContentDbContext>config, IUtcDateTimeProvider utcDateTimeProvider, IAgConfig agConfig)
        {
            _DbConfig = config;
            _UtcDateTimeProvider = utcDateTimeProvider;
            _AgConfig = agConfig;
        }

        public async Task Execute()
        {
            var now = _UtcDateTimeProvider.Now();
            
            var timeToDie = _DbConfig.Current.Set<ExposureKeySetContentEntity>()
                .Where(x => x.Release < now - TimeSpan.FromDays(_AgConfig.ExposureKeySetLifetimeDays))
                .ToList();

            await _DbConfig.Current.BulkDeleteAsync(timeToDie);
        }
    }
}