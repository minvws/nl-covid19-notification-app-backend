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
        private readonly Func<ContentDbContext> _ContentDbContext;
        private readonly IContentSigner _ContentSigner;
        private readonly ResignerLoggingExtensions _Logger;

        private string _ContentEntryName;
        private string _ToType;

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
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _ContentSigner = contentSigner ?? throw new ArgumentNullException(nameof(contentSigner));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Copy and sign all content of 'fromType' that has not already been re-signed.
        /// </summary>
        public async Task ExecuteAsync(string fromType, string toType, string contentEntryName)
        {
            _ToType = toType;
            _ContentEntryName = contentEntryName;

            var db = _ContentDbContext();

            var fromItems = db.Content.Where(x => x.Type == fromType).ToArray();
            var toItems = db.Content.Where(x => x.Type == toType).ToArray();
            var todo = fromItems.Except(toItems,  new ContentEntityComparer()).ToArray();

            _Logger.WriteReport(todo);

            foreach (var i in todo)
                await ReSignAsync(i);

            _Logger.WriteFinished();
        }

        private async Task ReSignAsync(ContentEntity item)
        {
            await using var db = _ContentDbContext();
            await using var tx = db.BeginTransaction();

            var content = await ReplaceSignatureAsync(item.Content);
            var e = new ContentEntity
            {
                Created = item.Created,
                Release = item.Release,
                ContentTypeName = item.ContentTypeName,
                Content = content,
                Type = _ToType,
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
                var content = archive.ReadEntry(_ContentEntryName);
                var sig = _ContentSigner.GetSignature(content);
                await archive.ReplaceEntryAsync(ZippedContentEntryNames.NLSignature, sig);
            }
            return m.ToArray();
        }
    }
}