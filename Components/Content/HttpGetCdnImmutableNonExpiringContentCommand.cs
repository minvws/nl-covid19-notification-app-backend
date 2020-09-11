using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class HttpGetCdnImmutableNonExpiringContentCommand
    {
        private readonly HttpGetCdnContentCommand _GetCommand;
        private readonly ImmutableNonExpiringContentCacheControlHeaderProcessor _CacheControlHeaderProcessor;

        public HttpGetCdnImmutableNonExpiringContentCommand(HttpGetCdnContentCommand getCommand, ImmutableNonExpiringContentCacheControlHeaderProcessor cacheControlHeaderProcessor)
        {
            _GetCommand = getCommand ?? throw new ArgumentNullException(nameof(getCommand));
            _CacheControlHeaderProcessor = cacheControlHeaderProcessor ?? throw new ArgumentNullException(nameof(cacheControlHeaderProcessor));
        }

        public async Task Execute(HttpContext httpContext, string type, string id)
        {
            var e = await _GetCommand.Execute(httpContext, type, id);
            _CacheControlHeaderProcessor.Execute(httpContext, e);
        }
    }
}