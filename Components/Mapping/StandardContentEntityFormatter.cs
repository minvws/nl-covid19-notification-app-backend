// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public class StandardContentEntityFormatter : IContentEntityFormatter
    {
        private readonly ZippedSignedContentFormatter _SignedFormatter;
        private readonly IPublishingId _PublishingId;
        private readonly IJsonSerializer _JsonSerializer;

        public StandardContentEntityFormatter(ZippedSignedContentFormatter signedFormatter, IPublishingId publishingId, IJsonSerializer jsonSerializer)
        {
            _SignedFormatter = signedFormatter ?? throw new ArgumentNullException(nameof(signedFormatter));
            _PublishingId = publishingId ?? throw new ArgumentNullException(nameof(publishingId));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        public async Task<TContentEntity> Fill<TContentEntity, TContent>(TContentEntity e, TContent c) where TContentEntity : ContentEntity
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (c == null) throw new ArgumentNullException(nameof(c));

            var contentJson = _JsonSerializer.Serialize(c);
            var contentBytes = Encoding.UTF8.GetBytes(contentJson);
            e.PublishingId = _PublishingId.Create(contentBytes);
            e.Content = await _SignedFormatter.SignedContentPacket(contentBytes);
            e.ContentTypeName = MediaTypeNames.Application.Zip;
            return e;
        }
    }
}