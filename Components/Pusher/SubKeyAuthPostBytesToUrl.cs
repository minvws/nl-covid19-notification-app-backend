using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class SubKeyAuthPostBytesToUrl
    {
        private readonly IReceiverConfig _Config;
        private readonly ILogger _Logger;

        public SubKeyAuthPostBytesToUrl(IReceiverConfig config, ILogger<SubKeyAuthPostBytesToUrl> logger)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="args"></param>
        /// <returns>true if written to the db.</returns>
        public async Task<bool> Execute(string uri, byte[] args)
        {
            if (string.IsNullOrWhiteSpace(uri)) throw new ArgumentException(nameof(uri));
            if (args == null) throw new ArgumentNullException(nameof(args));

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(Encoding.UTF8.GetString(args), Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            request.Headers.Add("Ocp-Apim-Subscription-Key", _Config.Password);

            var response = await client.SendAsync(request);

            return ConvertResponse(response);
        }

        private bool ConvertResponse(HttpResponseMessage response)
        {
            switch(response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return true;
                case HttpStatusCode.Conflict:
                    return false;
                default:
                    _Logger.LogError($"Status not handled - {response.StatusCode}, {response.ReasonPhrase}.");
                    throw new InvalidOperationException($"Status not handled.");
            };
        }
    }
}