using System;
using System.IO;
using System.Net;
using System.Net.Mime;

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
        public bool Execute(string uri, byte[] args)
        {
            var wr = WebRequest.CreateHttp(uri);
            wr.Method = "POST";
            wr.Headers.Add("content-type", MediaTypeNames.Application.Json);
            wr.Credentials = new NetworkCredential(_ReceiverConfig.Username, _ReceiverConfig.Password);
            wr.ContentLength = args.Length;
            var dataStream = wr.GetRequestStream();
            dataStream.Write(args, 0, args.Length);
            dataStream.Close();
            try
            {
                using var response = (HttpWebResponse)wr.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException ex)
            {
                using var response = (HttpWebResponse)ex.Response;
                if (response.StatusCode == HttpStatusCode.Conflict)
                    return false;
                
                throw;
            }
        }
    }
}