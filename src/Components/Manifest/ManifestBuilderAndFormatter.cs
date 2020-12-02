// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    //Reads and formats...
    public class ManifestBuilderAndFormatter
    {
        private readonly ManifestBuilder _ManifestBuilder;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ILogger _Logger;
        private readonly IContentEntityFormatter _Formatter; //new StandardContentEntityFormatter(new ZippedSignedContentFormatter(contentSigner), new StandardPublishingIdFormatter(), jsonSerializer1);

        public ManifestBuilderAndFormatter(ManifestBuilder manifestBuilder, IUtcDateTimeProvider dateTimeProvider, ILogger<ManifestBuilderAndFormatter> logger, IContentEntityFormatter formatter)
        {
            _ManifestBuilder = manifestBuilder ?? throw new ArgumentNullException(nameof(manifestBuilder));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public async Task<ContentEntity> ExecuteAsync()
        {
            var snapshot = _DateTimeProvider.Snapshot;
            var e = new ContentEntity
            {
                Created = snapshot,
                Release = snapshot,
                Type = ContentTypes.Manifest
            };
            _Logger.LogDebug("Build new manifest.");
            var content = await _ManifestBuilder.ExecuteAsync();
            _Logger.LogDebug("Format and sign new manifest.");
            await _Formatter.FillAsync(e, content);
            return e;
        }
    }
}