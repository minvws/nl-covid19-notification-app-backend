// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.GenericContent
{
    public class ContentInsertDbCommand
    {
        private readonly ContentDbContext _DbContext;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IPublishingId _PublishingId;
        private readonly ZippedSignedContentFormatter _SignedFormatter;

        public ContentInsertDbCommand(ContentDbContext dbContext, IUtcDateTimeProvider dateTimeProvider, IPublishingId publishingId, ZippedSignedContentFormatter signedFormatter)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _PublishingId = publishingId ?? throw new ArgumentNullException(nameof(publishingId));
            _SignedFormatter = signedFormatter ?? throw new ArgumentNullException(nameof(signedFormatter));
        }

        public async Task Execute(GenericContentArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var contentBytes = Encoding.UTF8.GetBytes(args.Json);

            var e = new GenericContentEntity
            {
                Created= _DateTimeProvider.Now(), //TODO audit stamp
                Release = args.Release,
                GenericType = args.GenericContentType,
                PublishingId = _PublishingId.Create(contentBytes),
                SignedContent = await _SignedFormatter.SignedContentPacket(contentBytes),
                SignedContentTypeName = MediaTypeNames.Application.Zip
            };

            await _DbContext.AddAsync(e);
        }
    }
}