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
        private readonly IReceiverConfig _ReceiverConfig;

        public BasicAuthPostBytesToUrl(IReceiverConfig receiverConfig)
        {
            _ReceiverConfig = receiverConfig;
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
            client.DefaultRequestHeaders.Add("Authorization",Convert.ToBase64String(Encoding.UTF8.GetBytes($"Basic {_ReceiverConfig.Username}:{_ReceiverConfig.Password}")));
            var response = await client.PostAsync(uri, new ByteArrayContent(args));

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.Conflict => false,
                _ => throw new InvalidOperationException()
            };
        }
    }
}