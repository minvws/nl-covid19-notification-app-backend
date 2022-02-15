// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class ContentInsertDbCommand
    {
        private readonly ContentDbContext _dbContext;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ZippedSignedContentFormatter _signedFormatter;

        public ContentInsertDbCommand(
            ContentDbContext dbContext,
            IUtcDateTimeProvider dateTimeProvider,
            ZippedSignedContentFormatter signedFormatter)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _signedFormatter = signedFormatter ?? throw new ArgumentNullException(nameof(signedFormatter));
        }

        public async Task ExecuteAsync(ContentArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            // Generate a new guid string, formatted without dashes
            var newPublishingId = Guid.NewGuid().ToString("N");

            var contentBytes = Encoding.UTF8.GetBytes(args.Json);

            var e = new ContentEntity
            {
                Created = _dateTimeProvider.Snapshot,
                Release = args.Release,
                Type = args.ContentType,
                PublishingId = newPublishingId,
                Content = await _signedFormatter.SignedContentPacketAsync(contentBytes)
            };

            await _dbContext.Content.AddAsync(e);
        }
    }
}
