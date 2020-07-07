using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class SubKeyAuthPostBytesToUrl
    {
        private readonly IReceiverConfig _Config;

        public SubKeyAuthPostBytesToUrl(IReceiverConfig receiverConfig)
        {
            _Config = receiverConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="args"></param>
        /// <returns>true if written to the db.</returns>
        public async Task<bool> Execute(string uri, byte[] args)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(Encoding.UTF8.GetString(args), Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            request.Headers.Add("Ocp-Apim-Subscription-Key", _Config.Password);
            
            var response = await client.SendAsync(request);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.Conflict => false,
                _ => throw new InvalidOperationException($"Status {response.StatusCode} not handled.")
            };
        }
    }
}