using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class BasicAuthPostBytesToUrl
    {
        private readonly IReceiverConfig _Config;

        public BasicAuthPostBytesToUrl(IReceiverConfig receiverConfig)
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
                Content = new ByteArrayContent(args)
            };

            request.Headers.Add("Authorization",$"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_Config.Username}:{_Config.Password}"))}");
            var response = await client.SendAsync(request);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.Conflict => false,
                _ => throw new InvalidOperationException()
            };
        }
    }
}