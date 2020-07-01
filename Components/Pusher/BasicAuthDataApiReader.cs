

using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class BasicAuthDataApiReader
    {
        private readonly IDataApiUrls _Config;

        public BasicAuthDataApiReader(IDataApiUrls config)
        {
            _Config = config;
        }

        public async Task<byte[]> Read(string uri)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization",$"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_Config.Username}:{_Config.Password}"))}");
            request.Headers.Add("Accept",MediaTypeNames.Application.Json);
            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Read failed.");

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}