// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;

public class ManifestBuilder
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IEksConfig _eksConfig;
    private readonly IUtcDateTimeProvider _dateTimeProvider;

    public ManifestBuilder(ContentDbContext contentDbContext, IEksConfig eksConfig, IUtcDateTimeProvider dateTimeProvider)
    {
        _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
        _eksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<ManifestContent> ExecuteAsync()
    {
        var snapshot = _dateTimeProvider.Snapshot;
        var from = snapshot - TimeSpan.FromDays(_eksConfig.LifetimeDays);
        return new ManifestContent
        {
            ExposureKeySets = await _contentDbContext.SafeGetActiveContentIdListAsync(ContentTypes.ExposureKeySet, from, snapshot),
            RiskCalculationParameters = await _contentDbContext.SafeGetLatestContentIdAsync(ContentTypes.RiskCalculationParameters, snapshot),
            AppConfig = await _contentDbContext.SafeGetLatestContentIdAsync(ContentTypes.AppConfig, snapshot),
            ResourceBundle = await _contentDbContext.SafeGetLatestContentIdAsync(ContentTypes.ResourceBundle, snapshot)
        };
    }
}
