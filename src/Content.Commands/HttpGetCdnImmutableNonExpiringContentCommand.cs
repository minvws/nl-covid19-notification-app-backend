// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class HttpGetCdnImmutableNonExpiringContentCommand
    {
        private readonly HttpGetCdnContentCommand _getCommand;
        private readonly IHttpResponseHeaderConfig _httpResponseHeaderConfig;

        public HttpGetCdnImmutableNonExpiringContentCommand(HttpGetCdnContentCommand getCommand, IHttpResponseHeaderConfig httpResponseHeaderConfig)
        {
            _getCommand = getCommand ?? throw new ArgumentNullException(nameof(getCommand));
            _httpResponseHeaderConfig = httpResponseHeaderConfig ?? throw new ArgumentNullException(nameof(httpResponseHeaderConfig));
        }

        public async Task ExecuteAsync(HttpContext httpContext, string type, string id)
        {
            var e = await _getCommand.ExecuteAsync(httpContext, type, id);
            if (e == null)
            {
                return;
            }

            httpContext.Response.Headers.Add("cache-control", _httpResponseHeaderConfig.ImmutableContentCacheControl);
            await httpContext.Response.Body.WriteAsync(e.Content);
        }
    }
}