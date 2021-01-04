// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class StandardContentEntityFormatter : IContentEntityFormatter
    {
        private readonly ZippedSignedContentFormatter _SignedFormatter;
        private readonly IPublishingIdService _PublishingIdService;
        private readonly IJsonSerializer _JsonSerializer;

        public StandardContentEntityFormatter(ZippedSignedContentFormatter signedFormatter, IPublishingIdService publishingIdService, IJsonSerializer jsonSerializer)
        {
            _SignedFormatter = signedFormatter ?? throw new ArgumentNullException(nameof(signedFormatter));
            _PublishingIdService = publishingIdService ?? throw new ArgumentNullException(nameof(publishingIdService));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        public async Task<TContentEntity> FillAsync<TContentEntity, TContent>(TContentEntity e, TContent c) where TContentEntity : ContentEntity
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (c == null) throw new ArgumentNullException(nameof(c));

            var contentJson = _JsonSerializer.Serialize(c);
            var contentBytes = Encoding.UTF8.GetBytes(contentJson);
            e.PublishingId = _PublishingIdService.Create(contentBytes);
            e.Content = await _SignedFormatter.SignedContentPacketAsync(contentBytes);
            e.ContentTypeName = MediaTypeNames.Application.Zip;
            return e;
        }
    }
}