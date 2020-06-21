using System.Net;
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
            var wc = new WebClient();
            wc.Credentials = new NetworkCredential(_Config.Username, _Config.Password);
            return await wc.DownloadDataTaskAsync(uri);
        }
    }
}