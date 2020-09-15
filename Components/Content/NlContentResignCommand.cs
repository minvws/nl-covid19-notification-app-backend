// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class NlContentResignCommand
    {
        private readonly Func<ContentDbContext> _ContentDbContext;
        private readonly IContentSigner _ContentSigner;
        
        private string _ContentEntryName;
        private string _ToType;
        private string _FromType;

        public NlContentResignCommand(Func<ContentDbContext> contentDbContext, IContentSigner contentSigner)
        {
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _ContentSigner = contentSigner ?? throw new ArgumentNullException(nameof(contentSigner));
        }

        public async Task Execute(string fromType, string toType, string contentEntryName)
        {
            _FromType = fromType;
            _ToType = toType;
            _ContentEntryName = contentEntryName;

            var db = _ContentDbContext();
            var fromItems = db.Content.Where(x => x.Type == fromType).Select(x => x.PublishingId).ToArray();
            var toItems = db.Content.Where(x => x.Type == toType).Select(x => x.PublishingId).ToArray();
            var todo = fromItems.Except(toItems).ToArray();

            foreach (var i in todo)
            {
                await Resign(i);
            }
        }

        private async Task Resign(string publishingId)
        {
            await using var db = _ContentDbContext();
            await using var tx = db.BeginTransaction();
            var item = db.Content.Single(x => x.Type == _FromType && x.PublishingId == publishingId);
            var content = await ReplaceSig(item.Content);
            var e = new ContentEntity
            {
                Created = item.Created,
                Release = item.Release,
                ContentTypeName = MediaTypeNames.Application.Zip,
                Content = content,
                Type = _ToType,
                PublishingId = publishingId
            };
            await db.Content.AddAsync(e);
            db.SaveAndCommit();
        }

        private async Task<byte[]> ReplaceSig(byte[] archiveBytes)
        {
            await using var m = new MemoryStream(archiveBytes);
            using (var archive = new ZipArchive(m, ZipArchiveMode.Update, true))
            {
                var content = archive.ReadEntry(_ContentEntryName);
                var sig = _ContentSigner.GetSignature(content);
                await archive.ReplaceEntry(ZippedContentEntryNames.NLSignature, sig);
            }
            return m.ToArray();
        }
    }
}