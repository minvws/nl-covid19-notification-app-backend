// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class HttpGetCdnEksCommand
    {
        private readonly HttpGetCdnContentCommand _getCommand;
        private readonly EksCacheControlHeaderProcessor _cacheControlHeaderProcessor;

        public HttpGetCdnEksCommand(HttpGetCdnContentCommand getCommand, EksCacheControlHeaderProcessor cacheControlHeaderProcessor)
        {
            _getCommand = getCommand ?? throw new ArgumentNullException(nameof(getCommand));
            _cacheControlHeaderProcessor = cacheControlHeaderProcessor ?? throw new ArgumentNullException(nameof(cacheControlHeaderProcessor));
        }

        public async Task ExecuteAsync(HttpContext httpContext, string type, string id)
        {
            var e = await _getCommand.ExecuteAsync(httpContext, type, id);
            if (e == null)
            {
                return;
            }

            _cacheControlHeaderProcessor.Execute(httpContext, e);
            await httpContext.Response.Body.WriteAsync(e.Content);
        }
    }
}
