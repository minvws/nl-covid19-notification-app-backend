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
    public class ManifestBuilderV3
    {
        private readonly ContentDbContext _ContentDbContext;
        private readonly IEksConfig _EksConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public ManifestBuilderV3(ContentDbContext contentDbContext, IEksConfig eksConfig, IUtcDateTimeProvider dateTimeProvider)
        {
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _EksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task<ManifestContent> Execute()
        {
            var snapshot = _DateTimeProvider.Snapshot;
            var from = snapshot - TimeSpan.FromDays(_EksConfig.LifetimeDays);
            return new ManifestContent
            {
                ExposureKeySets = await _ContentDbContext.SafeGetActiveContentIdListAsync(ContentTypes.ExposureKeySet, from, snapshot),
                RiskCalculationParameters = await _ContentDbContext.SafeGetLatestContentIdAsync(ContentTypes.RiskCalculationParameters, snapshot),
                AppConfig = await _ContentDbContext.SafeGetLatestContentIdAsync(ContentTypes.AppConfig, snapshot),
                ResourceBundle = await _ContentDbContext.SafeGetLatestContentIdAsync(ContentTypes.ResourceBundleV3, snapshot)
            };
        }
    }
}