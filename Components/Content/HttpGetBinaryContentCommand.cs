// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Used in the Cdn Data API in the Datacentre/Business Zone
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpGetBinaryContentCommand<T> where T : ContentEntity
    {
        private readonly IReader<T> _SafeReader;

        public HttpGetBinaryContentCommand(IReader<T> safeReader)
        {
            _SafeReader = safeReader;
        }

        public IActionResult Execute(string id)
        {
            //TODO BEGIN this is not the proper converter/validator - see IPublishingId
            if (string.IsNullOrWhiteSpace(id))
                return new BadRequestResult();

            if (Convert.TryFromBase64String(id, new Span<byte>(), out var length) && length == 256) //TODO config
                return new BadRequestResult();
            //TODO END this is not the proper converter/validator - see IPublishingId

            var e = _SafeReader.Execute(id);

            if (e == null)
                return new NotFoundResult();

            var r = new BinaryContentResponse
            {
                LastModified = e.Release,
                PublishingId = e.PublishingId,
                ContentTypeName = e.ContentTypeName,
                Content = e.Content
            };

            return new OkObjectResult(r);
        }
    }
}