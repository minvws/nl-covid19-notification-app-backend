// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using IJsonSerializer = NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping.IJsonSerializer;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestUpdateCommandResult
    {
        public bool Existing { get; set; }

        public bool Updated { get; set; }
    }

    public class ManifestUpdateCommand
    {
        private readonly ManifestBuilder _Builder;
        private readonly Func<ContentDbContext> _ContentDbProvider;
        private readonly ILogger<ManifestUpdateCommand> _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IJsonSerializer _JsonSerializer;
        private readonly IContentEntityFormatter _Formatter;

        private readonly ManifestUpdateCommandResult _Result = new ManifestUpdateCommandResult();
        private ContentDbContext _ContentDb;

        public ManifestUpdateCommand(ManifestBuilder builder, Func<ContentDbContext> contentDbProvider, ILogger<ManifestUpdateCommand> logger, IUtcDateTimeProvider dateTimeProvider, IJsonSerializer jsonSerializer, IContentEntityFormatter formatter)
        {
            _Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _ContentDbProvider = contentDbProvider ?? throw new ArgumentNullException(nameof(contentDbProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public async Task Execute()
        {
            if (_ContentDb != null)
                throw new InvalidOperationException("Command already used.");

            _ContentDb = _ContentDbProvider();
            await using var tx = _ContentDb.BeginTransaction();
            var candidate = await _Builder.Execute();

            if (!await WriteCandidate(candidate))
            {
                _Logger.LogInformation("Manifest does NOT require updating.");
                return;
            }

            _Logger.LogInformation("Manifest updating.");

            var snapshot = _DateTimeProvider.Snapshot;
            var e = new ContentEntity
            {
                Created = snapshot,
                Release = snapshot,
                Type = ContentTypes.Manifest
            };
            await _Formatter.Fill(e, candidate);

            _Result.Updated = true;

            _ContentDb.Add(e);
            _ContentDb.SaveAndCommit();

            _Logger.LogInformation("Manifest updated.");
        }

        private async Task<bool> WriteCandidate(ManifestContent candidate)
        {
            var existingContent = await _ContentDb.SafeGetLatestContent(ContentTypes.Manifest, _DateTimeProvider.Snapshot);
            if (existingContent == null)
                return true;

            _Result.Existing = true;
            var existingManifest = ParseContent(existingContent.Content);
            return !candidate.Equals(existingManifest);
        }

        private ManifestContent ParseContent(byte[] formattedContent)
        {
            using var readStream = new MemoryStream(formattedContent);
            using var zip = new ZipArchive(readStream);
            var entry = zip.GetEntry(ZippedSignedContentFormatter.ContentEntryName);
            using var entryStream = entry.Open();
            using var resultStream = new MemoryStream();
            entryStream.CopyTo(resultStream);
            return _JsonSerializer.Deserialize<ManifestContent>(Encoding.UTF8.GetString(resultStream.ToArray()));
        }
    }
}