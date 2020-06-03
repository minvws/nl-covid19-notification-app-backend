// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AgProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class GetActiveExposureKeySetsListCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbConfig;
        private readonly IAgConfig _AgConfig;

        public GetActiveExposureKeySetsListCommand(IDbContextProvider<ExposureContentDbContext>dbConfig, IAgConfig agConfig)
        {
            _DbConfig = dbConfig;
            _AgConfig = agConfig;
        }

        public AgExposureKeySetsConfig Execute()
        {
            var expired = new StandardUtcDateTimeProvider().Now() - TimeSpan.FromDays(_AgConfig.ExposureKeySetLifetimeDays);
            var activeExposureKeySets = _DbConfig.Current.Set<ExposureKeySetContentEntity>()
                .Where(x => x.Release > expired)
                .Select(x => x.PublishingId)
                .ToArray();

            return new AgExposureKeySetsConfig
            {
                Ids = activeExposureKeySets
            };
        }
    }
}