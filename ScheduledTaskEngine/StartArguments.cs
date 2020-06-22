using System;
using System.Net.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ScheduledTaskEngine
{
    internal class StartArguments
    {
        public HttpMethod HttpMethod { get; set; }
        public Uri Uri { get; set; }
        public string AuthorizationHeader { get; set; }
        public bool Valid => HttpMethod != null && Uri != null;
    }
}