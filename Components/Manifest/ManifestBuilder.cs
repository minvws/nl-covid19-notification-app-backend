// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestBuilder
    {
        private readonly ContentDbContext _ContentDbContext;
        private readonly IGaenContentConfig _GaenContentConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public ManifestBuilder(ContentDbContext contentDbContext, IGaenContentConfig gaenContentConfig, IUtcDateTimeProvider dateTimeProvider)
        {
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _GaenContentConfig = gaenContentConfig ?? throw new ArgumentNullException(nameof(gaenContentConfig));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task<ManifestContent> Execute()
        {
            var now = _DateTimeProvider.Now();
            var lo = now - TimeSpan.FromDays(_GaenContentConfig.ExposureKeySetLifetimeDays);
            return new ManifestContent
            { 
                ExposureKeySets = await _ContentDbContext.SafeGetActiveContentIdList(ContentTypes.ExposureKeySet, lo, now),
                RiskCalculationParameters = await _ContentDbContext.SafeGetLatestContentId(ContentTypes.RiskCalculationParameters, now),
                AppConfig = await _ContentDbContext.SafeGetLatestContentId(ContentTypes.AppConfig, now)
            };
        }
    }
}