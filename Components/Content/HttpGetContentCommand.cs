// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class HttpGetContentCommand<T> where T : ContentEntity
    {
        private readonly IDbContextProvider<ExposureContentDbContext> _DbContextProvider;

        public HttpGetContentCommand(IDbContextProvider<ExposureContentDbContext> dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public IActionResult Execute(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new BadRequestResult();

            if (Convert.TryFromBase64String(id, new Span<byte>(), out var length) && length == 256) //TODO config
                return new BadRequestResult();

            //TODO anything else to mitigate DDOS?

            var e = _DbContextProvider.Current.Set<T>()
                .SingleOrDefault(x => x.PublishingId == id);

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