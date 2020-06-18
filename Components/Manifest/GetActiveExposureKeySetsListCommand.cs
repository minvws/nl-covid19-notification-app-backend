// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class GetActiveExposureKeySetsListCommand
    {
        private readonly ExposureContentDbContext _DbConfig;
        private readonly IGaenContentConfig _GaenContentConfig;

        public GetActiveExposureKeySetsListCommand(ExposureContentDbContext dbConfig, IGaenContentConfig gaenContentConfig)
        {
            _DbConfig = dbConfig;
            _GaenContentConfig = gaenContentConfig;
        }

        public string[] Execute()
        {
            //TODO cant test this - inject!
            var expired = new StandardUtcDateTimeProvider().Now() - TimeSpan.FromDays(_GaenContentConfig.ExposureKeySetLifetimeDays);
            var result = _DbConfig.Set<ExposureKeySetContentEntity>()
                .Where(x => x.Release > expired)
                .Select(x => x.PublishingId)
                .ToArray();

            return result;
        }
    }
}