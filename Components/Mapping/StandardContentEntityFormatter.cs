// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

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

            var j = _JsonSerializer.Serialize(c);
            e.Content = Encoding.UTF8.GetBytes(j);
            e.PublishingId = _PublishingId.Create(e.Content);
            e.ContentTypeName = MediaTypeNames.Application.Json;
            e.SignedContent = await _SignedFormatter.SignedContentPacket(e.Content);
            e.SignedContentTypeName = MediaTypeNames.Application.Zip;
            return e;
        }
    }
}