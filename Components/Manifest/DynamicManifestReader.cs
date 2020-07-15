// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    //Reads and formats...
    public class DynamicManifestReader
    {
        private readonly ManifestBuilder _ManifestBuilder;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IContentSigner _ContentSigner;
        private readonly ILogger _Logger; //Actually not used.
        private readonly IJsonSerializer _JsonSerializer;

        public DynamicManifestReader(ManifestBuilder manifestBuilder, IUtcDateTimeProvider dateTimeProvider, IContentSigner contentSigner, ILogger logger, IJsonSerializer jsonSerializer)
        {
            _ManifestBuilder = manifestBuilder ?? throw new ArgumentNullException(nameof(manifestBuilder));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ContentSigner = contentSigner ?? throw new ArgumentNullException(nameof(contentSigner));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        public async Task<ManifestEntity?> Execute()
        {
            var e = new ManifestEntity
            {
                Release = _DateTimeProvider.Now(),
            };
            _Logger.LogDebug("Build new manifest.");
            var content = _ManifestBuilder.Execute();
            //TODO should be injected...
            _Logger.LogDebug("Format and sign new manifest.");
            var formatter = new StandardContentEntityFormatter(new ZippedSignedContentFormatter(_ContentSigner), new StandardPublishingIdFormatter(), _JsonSerializer);
            await formatter.Fill(e, content); //TODO add release date as a parameter
            return e;
        }
    }
}