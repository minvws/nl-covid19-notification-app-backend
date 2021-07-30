// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    //TODO add ticket - split up into explicit commands for each version.
    public class ManifestUpdateCommand
    {
        private readonly ManifestV2Builder _v2Builder; //Todo: rename classes to ManifestVxBuilder
        private readonly ManifestV3Builder _v3Builder;
        private readonly ManifestV4Builder _v4Builder;
        private readonly ContentDbContext _contentDbContext;
        private readonly ManifestUpdateCommandLoggingExtensions _logger;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Func<IContentEntityFormatter> _formatter;

        public ManifestUpdateCommand(
            ManifestV2Builder v2Builder,
            ManifestV3Builder v3Builder,
            ManifestV4Builder v4Builder,
            ContentDbContext contentDbContext,
            ManifestUpdateCommandLoggingExtensions logger,
            IUtcDateTimeProvider dateTimeProvider,
            IJsonSerializer jsonSerializer,
            Func<IContentEntityFormatter> formatter)
        {
            _v2Builder = v2Builder ?? throw new ArgumentNullException(nameof(v2Builder));
            _v3Builder = v3Builder ?? throw new ArgumentNullException(nameof(v3Builder));
            _v4Builder = v4Builder ?? throw new ArgumentNullException(nameof(v4Builder));
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        //ManifestV1 is no longer supported.
        public async Task ExecuteV2Async() => await Execute(async () => await _v2Builder.ExecuteAsync(), ContentTypes.ManifestV2);
        public async Task ExecuteV3Async() => await Execute(async () => await _v3Builder.ExecuteAsync(), ContentTypes.ManifestV3);
        public async Task ExecuteV4Async() => await Execute(async () => await _v4Builder.ExecuteAsync(), ContentTypes.ManifestV4);

        public async Task ExecuteAllAsync()
        {
            await ExecuteV2Async();
            await ExecuteV3Async();
            await ExecuteV4Async();
        }

        private async Task Execute<T>(Func<Task<T>> build, ContentTypes contentType) where T : IEquatable<T>
        {
            var snapshot = _dateTimeProvider.Snapshot;

            await using var tx = _contentDbContext.BeginTransaction();

            var currentManifestData = await _contentDbContext.SafeGetLatestContentAsync(contentType, snapshot);
            var candidateManifest = await build();

            if (currentManifestData != null)
            {
                var currentManifest = ParseContent<T>(currentManifestData.Content);

                if (candidateManifest.Equals(currentManifest))
                {
                    // If current manifest equals existing manifest, do nothing
                    _logger.WriteUpdateNotRequired();

                    return;
                }

                // If current manifest does not equal existing manifest, then replace current manifest.
                _contentDbContext.Remove(currentManifestData);
            }

            _logger.WriteStart();

            var contentEntity = new ContentEntity
            {
                Created = snapshot,
                Release = snapshot,
                Type = contentType
            };
            await _formatter().FillAsync(contentEntity, candidateManifest);

            _contentDbContext.Add(contentEntity);
            _contentDbContext.SaveAndCommit();

            _logger.WriteFinished();
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
