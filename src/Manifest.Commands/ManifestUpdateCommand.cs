// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class ManifestUpdateCommand : BaseCommand
    {
        private readonly ManifestBuilder _builder;
        private readonly ContentDbContext _contentDbContext;
        private readonly ILogger _logger;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IContentEntityFormatter _formatter;

        public ManifestUpdateCommand(
            ManifestBuilder builder,
            ContentDbContext contentDbContext,
            ILogger<ManifestUpdateCommand> logger,
            IUtcDateTimeProvider dateTimeProvider,
            IJsonSerializer jsonSerializer,
            IContentEntityFormatter formatter)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            var snapshot = _dateTimeProvider.Snapshot;

            await using var tx = _contentDbContext.BeginTransaction();

            var currentManifestData = await _contentDbContext.SafeGetLatestContentAsync(ContentTypes.Manifest, snapshot);
            var candidateManifest = await _builder.ExecuteAsync();

            if (currentManifestData != null)
            {
                var currentManifest = ParseContent<ManifestContent>(currentManifestData.Content);

                if (candidateManifest.Equals(currentManifest))
                {
                    // If current manifest equals existing manifest, do nothing
                    _logger.LogInformation("Manifest does NOT require updating.");

                    //TODO: don't return null ;(
                    return null;
                }

                // If current manifest does not equal existing manifest, then replace current manifest.
                _contentDbContext.Remove(currentManifestData);
            }

            _logger.LogInformation("Manifest updating.");

            var contentEntity = new ContentEntity
            {
                Created = snapshot,
                Release = snapshot,
                Type = ContentTypes.Manifest
            };
            await _formatter.FillAsync(contentEntity, candidateManifest);

            _contentDbContext.Add(contentEntity);
            _contentDbContext.SaveAndCommit();

            _logger.LogInformation("Manifest updated.");

            //TODO: don't return null ;(
            return null;
        }

        private T ParseContent<T>(byte[] formattedContent)
        {
            using var readStream = new MemoryStream(formattedContent);
            using var zip = new ZipArchive(readStream);
            var content = zip.ReadEntry(ZippedContentEntryNames.Content);
            return _jsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(content));
        }
    }
}
