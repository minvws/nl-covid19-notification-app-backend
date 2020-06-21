using System.IO;
using System.Net;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class PushContentByUrl
    {
        public bool Execute(string uri, byte[] args)
        {
            var buffer = new MemoryStream();
            ProtoBuf.Serializer.Serialize(buffer, args);
            var wr = WebRequest.CreateHttp(uri);
            wr.Method = "POST";
            var data = buffer.ToArray();
            wr.ContentLength = data.Length;
            var dataStream = wr.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();
            var response = (HttpWebResponse)wr.GetResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}