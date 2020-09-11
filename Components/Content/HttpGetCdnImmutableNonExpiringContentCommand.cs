using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class HttpGetCdnImmutableNonExpiringContentCommand
    {
        private readonly HttpGetCdnContentCommand _GetCommand;
        private readonly IHttpResponseHeaderConfig _HttpResponseHeaderConfig;

        public HttpGetCdnImmutableNonExpiringContentCommand(HttpGetCdnContentCommand getCommand, IHttpResponseHeaderConfig httpResponseHeaderConfig)
        {
            _GetCommand = getCommand ?? throw new ArgumentNullException(nameof(getCommand));
            _HttpResponseHeaderConfig = httpResponseHeaderConfig ?? throw new ArgumentNullException(nameof(httpResponseHeaderConfig));
        }

        public async Task Execute(HttpContext httpContext, string type, string id)
        {
            var e = await _GetCommand.Execute(httpContext, type, id);
            if (e == null) return;
            httpContext.Response.Headers.Add("cache-control", _HttpResponseHeaderConfig.ImmutableContentCacheControl);
            await httpContext.Response.Body.WriteAsync(e.Content);
        }
    }
}