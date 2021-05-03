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
        private readonly ManifestV2Builder _V2Builder; //Todo: rename classes to ManifestVxBuilder
        private readonly ManifestV3Builder _V3Builder;
        private readonly ManifestV4Builder _V4Builder;
        private readonly Func<ContentDbContext> _ContentDbProvider;
        private readonly ManifestUpdateCommandLoggingExtensions _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IJsonSerializer _JsonSerializer;
        private readonly Func<IContentEntityFormatter> _Formatter;

        private ContentDbContext _ContentDb;

        public ManifestUpdateCommand(
            ManifestV2Builder v2Builder,
            ManifestV3Builder v3Builder,
            ManifestV4Builder v4Builder,
            Func<ContentDbContext> contentDbProvider,
            ManifestUpdateCommandLoggingExtensions logger,
            IUtcDateTimeProvider dateTimeProvider,
            IJsonSerializer jsonSerializer,
            Func<IContentEntityFormatter> formatter)
        {
            _V2Builder = v2Builder ?? throw new ArgumentNullException(nameof(v2Builder));
            _V3Builder = v3Builder ?? throw new ArgumentNullException(nameof(v3Builder));
            _V4Builder = v4Builder ?? throw new ArgumentNullException(nameof(v4Builder));
            _ContentDbProvider = contentDbProvider ?? throw new ArgumentNullException(nameof(contentDbProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        //ManifestV1 is no longer supported.
        public async Task ExecuteV2Async() => await Execute(async () => await _V2Builder.ExecuteAsync(), ContentTypes.ManifestV2);
        public async Task ExecuteV3Async() => await Execute(async () => await _V3Builder.ExecuteAsync(), ContentTypes.ManifestV3);
        public async Task ExecuteV4Async() => await Execute(async () => await _V4Builder.ExecuteAsync(), ContentTypes.ManifestV4);

        public async Task ExecuteAllAsync()
        {
            await ExecuteV2Async();
            await ExecuteV3Async();
            await ExecuteV4Async();
        }

        private async Task Execute<T>(Func<Task<T>> build, string contentType) where T: IEquatable<T>
        {
            var snapshot = _DateTimeProvider.Snapshot;

            _ContentDb ??= _ContentDbProvider();
            await using var tx = _ContentDb.BeginTransaction();
            
            var currentManifestData = await _ContentDb.SafeGetLatestContentAsync(contentType, snapshot);
            var candidateManifest = await build();

            if (currentManifestData != null)
            {
                var currentManifest = ParseContent<T>(currentManifestData.Content);

                if (candidateManifest.Equals(currentManifest))
                {
                    // If current manifest equals existing manifest, do nothing
                    _Logger.WriteUpdateNotRequired();
                    
                    return;
                }

                // If current manifest does not equal existing manifest, then replace current manifest.
                _ContentDb.Remove(currentManifestData);
            }
            
            _Logger.WriteStart();

            var contentEntity = new ContentEntity
            {
                Created = snapshot,
                Release = snapshot,
                Type = contentType
            };
            await _Formatter().FillAsync(contentEntity, candidateManifest);

            _ContentDb.Add(contentEntity);
            _ContentDb.SaveAndCommit();
            
            _Logger.WriteFinished();
        }
        
        private T ParseContent<T>(byte[] formattedContent)
        {
            using var readStream = new MemoryStream(formattedContent);
            using var zip = new ZipArchive(readStream);
            var content = zip.ReadEntry(ZippedContentEntryNames.Content);
            return _JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(content));
        }
    }
}