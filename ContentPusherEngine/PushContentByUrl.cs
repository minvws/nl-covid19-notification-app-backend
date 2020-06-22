using System.IO;
using System.Net;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class PushContentByUrl
    {
        public bool Execute(string uri, byte[] args)
        {
            var wr = WebRequest.CreateHttp(uri);
            wr.Method = "POST";
            wr.ContentLength = args.Length;
            var dataStream = wr.GetRequestStream();
            dataStream.Write(args, 0, args.Length);
            dataStream.Close();
            var response = (HttpWebResponse)wr.GetResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}