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
        private readonly ManifestBuilder _Builder;
        private readonly ManifestBuilderV3 _BuilderForV3;
        private readonly ManifestBuilderV4 _BuilderForV4;
        private readonly Func<ContentDbContext> _ContentDbProvider;
        private readonly ManifestUpdateCommandLoggingExtensions _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IJsonSerializer _JsonSerializer;
        private readonly Func<IContentEntityFormatter> _FormatterForV3;

        private readonly ManifestUpdateCommandResult _Result = new ManifestUpdateCommandResult();
        private ContentDbContext _ContentDb;

        public ManifestUpdateCommand(
            ManifestBuilder builder,
            ManifestBuilderV3 builderForV3,
            ManifestBuilderV4 builderForV4,
            Func<ContentDbContext> contentDbProvider,
            ManifestUpdateCommandLoggingExtensions logger,
            IUtcDateTimeProvider dateTimeProvider,
            IJsonSerializer jsonSerializer,
            IContentEntityFormatter formatter,
            Func<IContentEntityFormatter> formatterForV3
            )
        {
            _Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _BuilderForV3 = builderForV3 ?? throw new ArgumentNullException(nameof(builderForV3));
            _BuilderForV4 = builderForV4 ?? throw new ArgumentNullException(nameof(builderForV4));
            _ContentDbProvider = contentDbProvider ?? throw new ArgumentNullException(nameof(contentDbProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _FormatterForV3 = formatterForV3 ?? throw new ArgumentNullException(nameof(formatterForV3));
        }

        public async Task ExecuteV1Async() => await ExecuteForVxxx(async () => await _Builder.ExecuteAsync(), ContentTypes.Manifest);

        //There is no V2.
        
        public async Task ExecuteV3Async() => await ExecuteForVxxx(async () => await _BuilderForV3.ExecuteAsync(), ContentTypes.ManifestV3);
        public async Task ExecuteV4Async() => await ExecuteForVxxx(async () => await _BuilderForV4.ExecuteAsync(), ContentTypes.ManifestV4);

        private async Task ExecuteForVxxx<T>(Func<Task<T>> build, string contentType) where T: IEquatable<T>
        {
            _ContentDb ??= _ContentDbProvider();

            await using var tx = _ContentDb.BeginTransaction();
            var candidate = await build();

            if (await ShouldLeaveCurrentManifestAsync(candidate, contentType))
            {
                _Logger.WriteUpdateNotRequired();
                return;
            }

            _Logger.WriteStart();

            var snapshot = _DateTimeProvider.Snapshot;
            var contentEntity = new ContentEntity
            {
                Created = snapshot,
                Release = snapshot,
                Type = contentType
            };
            await _FormatterForV3().FillAsync(contentEntity, candidate);

            _Result.Updated = true;

            _ContentDb.Add(contentEntity);
            
            _ContentDb.SaveAndCommit();

            _Logger.WriteFinished();
        }

        public async Task ExecuteAllAsync()
        {
            await ExecuteV1Async();
            await ExecuteV3Async();
            await ExecuteV4Async();
        }

        private async Task<bool> ShouldLeaveCurrentManifestAsync<T>(T candidate, string contentType) where T: IEquatable<T>
        {
            var existingContent = await _ContentDb.SafeGetLatestContentAsync(contentType, _DateTimeProvider.Snapshot);
            
            if (existingContent == null)
            {
                _Result.Existing = false;
                return false;
            }

            _Result.Existing = true;
            var existingManifest = ParseContent<T>(existingContent.Content);

            // If current manifest equals existing manifest, do nothing
            if (candidate.Equals(existingManifest))
            {
                return true;
            }

            // If current manifest NOT equals existing manifest, the current manifest should be replaced thus remove it.
            _ContentDb.Remove(existingContent);
            return false;
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