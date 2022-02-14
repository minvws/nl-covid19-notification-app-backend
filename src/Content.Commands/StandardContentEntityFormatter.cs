// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class StandardContentEntityFormatter : IContentEntityFormatter
    {
        private readonly ZippedSignedContentFormatter _signedFormatter;
        private readonly IJsonSerializer _jsonSerializer;

        public StandardContentEntityFormatter(ZippedSignedContentFormatter signedFormatter, IJsonSerializer jsonSerializer)
        {
            _signedFormatter = signedFormatter ?? throw new ArgumentNullException(nameof(signedFormatter));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        public async Task<TContentEntity> FillAsync<TContentEntity, TContent>(TContentEntity e, TContent c) where TContentEntity : ContentEntity
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (c == null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            // Generate a new guid string, formatted without dashes
            var newPublishingId = Guid.NewGuid().ToString("N");

            var contentJson = _jsonSerializer.Serialize(c);
            var contentBytes = Encoding.UTF8.GetBytes(contentJson);
            e.PublishingId = newPublishingId;
            e.Content = await _signedFormatter.SignedContentPacketAsync(contentBytes);
            return e;
        }
    }
}
