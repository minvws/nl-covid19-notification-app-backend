using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class HttpGetCdnEksCommand
    {
        private readonly HttpGetCdnContentCommand _GetCommand;
        private readonly EksCacheControlHeaderProcessor _CacheControlHeaderProcessor;

        public HttpGetCdnEksCommand(HttpGetCdnContentCommand getCommand, EksCacheControlHeaderProcessor cacheControlHeaderProcessor)
        {
            _GetCommand = getCommand ?? throw new ArgumentNullException(nameof(getCommand));
            _CacheControlHeaderProcessor = cacheControlHeaderProcessor ?? throw new ArgumentNullException(nameof(cacheControlHeaderProcessor));
        }

        public async Task Execute(HttpContext httpContext, string type, string id)
        {
            var e = await _GetCommand.Execute(httpContext, type, id);
            if (e == null) return;
            _CacheControlHeaderProcessor.Execute(httpContext, e);
            await httpContext.Response.Body.WriteAsync(e.Content);
        }
    }
}