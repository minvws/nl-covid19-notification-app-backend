// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Used in the Cdn Data API in the Datacentre/Business Zone
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpGetBinaryContentCommand<T> where T : ContentEntity
    {
        private readonly IReader<T> _SafeReader;
        private readonly IPublishingId _PublishingId;

        public HttpGetBinaryContentCommand(IReader<T> safeReader, IPublishingId publishingId)
        {
            _SafeReader = safeReader;
            _PublishingId = publishingId;
        }

        public IActionResult Execute(string id, bool signed = false)
        {
            if (!_PublishingId.Validate(id))
                return new BadRequestResult();

            var e = _SafeReader.Execute(id);

            if (e == null)
                return new NotFoundResult();

            var r = new BinaryContentResponse
            {
                LastModified = e.Release,
                PublishingId = e.PublishingId,
                ContentTypeName = signed ? e.SignedContentTypeName : e.ContentTypeName,
                Content = signed ? e.SignedContent : e.Content
            };

            return new OkObjectResult(r);
        }
    }
}