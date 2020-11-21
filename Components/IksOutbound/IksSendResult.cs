using System.Net;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound
{
    public class IksSendResult
    {
        public HttpStatusCode? StatusCode { get; set; }
        public bool Exception { get; set; }
    }
}