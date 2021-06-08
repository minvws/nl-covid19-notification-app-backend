// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class NlContentResignCommand
    {
        private readonly Func<ContentDbContext> _contentDbContext;
        private readonly IContentSigner _contentSigner;
        private readonly ResignerLoggingExtensions _logger;

        private string _contentEntryName;
        private string _toType;

        /// <summary>
        /// Comparer ensures content is equivalent so that items are not re-signed more than once
        /// </summary>
        private class ContentEntityComparer : IEqualityComparer<ContentEntity>
        {
            public bool Equals(ContentEntity left, ContentEntity right)
             => left.Created == right.Created
               && left.Release == right.Release
               && left.PublishingId == right.PublishingId;

            public int GetHashCode(ContentEntity obj) => HashCode.Combine(obj.Created, obj.Release, obj.PublishingId);
        }

        public NlContentResignCommand(Func<ContentDbContext> contentDbContext, IContentSigner contentSigner, ResignerLoggingExtensions logger)
        {
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _contentSigner = contentSigner ?? throw new ArgumentNullException(nameof(contentSigner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Copy and sign all content of 'fromType' that has not already been re-signed.
        /// </summary>
        public async Task ExecuteAsync(string fromType, string toType, string contentEntryName)
        {
            _toType = toType;
            _contentEntryName = contentEntryName;

            var db = _contentDbContext();

            var fromItems = db.Content.Where(x => x.Type == fromType).ToArray();
            var toItems = db.Content.Where(x => x.Type == toType).ToArray();
            var todo = fromItems.Except(toItems, new ContentEntityComparer()).ToArray();

            _logger.WriteReport(todo);

            foreach (var i in todo)
            {
                await ReSignAsync(i);
            }

            _logger.WriteFinished();
        }

        private async Task ReSignAsync(ContentEntity item)
        {
            await using var db = _contentDbContext();
            await using var tx = db.BeginTransaction();

            var content = await ReplaceSignatureAsync(item.Content);
            var e = new ContentEntity
            {
                Created = item.Created,
                Release = item.Release,
                ContentTypeName = item.ContentTypeName,
                Content = content,
                Type = _toType,
                PublishingId = item.PublishingId
            };
            await db.Content.AddAsync(e);
            db.SaveAndCommit();
        }

        private async Task<byte[]> ReplaceSignatureAsync(byte[] archiveBytes)
        {
            await using var m = new MemoryStream();
            m.Write(archiveBytes, 0, archiveBytes.Length);
            using (var archive = new ZipArchive(m, ZipArchiveMode.Update, true))
            {
                var content = archive.ReadEntry(_contentEntryName);
                var sig = _contentSigner.GetSignature(content);
                await archive.ReplaceEntryAsync(ZippedContentEntryNames.NlSignature, sig);
            }
            return m.ToArray();
        }
    }
}
