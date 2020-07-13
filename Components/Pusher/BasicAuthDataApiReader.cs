

using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class BasicAuthDataApiReader
    {
        private readonly IDataApiUrls _Config;
        private readonly ILogger _Logger;

        public BasicAuthDataApiReader(IDataApiUrls config, ILogger logger)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<byte[]> Read(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) 
                throw new ArgumentException(nameof(uri));

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization",$"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_Config.Username}:{_Config.Password}"))}");
            request.Headers.Add("Accept",MediaTypeNames.Application.Json);
            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _Logger.Error($"Read from CDN Data API failed - HttpStatus:{response.StatusCode}, Body:{response.Content}.");
                throw new InvalidOperationException("Read failed.");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}