// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public class StandardContentEntityFormatter : IContentEntityFormatter
    {
        private readonly ZippedSignedContentFormatter _SignedFormatter;
        private readonly IPublishingId _PublishingId;

        public StandardContentEntityFormatter(ZippedSignedContentFormatter signedFormatter, IPublishingId publishingId)
        {
            _SignedFormatter = signedFormatter;
            _PublishingId = publishingId;
        }

        public async Task<TContentEntity> Fill<TContentEntity, TContent>(TContentEntity e, TContent c) where TContentEntity : ContentEntity
        {
            e.Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(c));
            e.PublishingId = _PublishingId.Create(e.Content);
            e.ContentTypeName = MediaTypeNames.Application.Json;
            e.SignedContent = await _SignedFormatter.SignedContentPacket(e.Content);
            e.SignedContentTypeName = MediaTypeNames.Application.Zip;
            return e;
        }
    }
}