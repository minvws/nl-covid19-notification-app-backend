using System.Net;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class IksSendResult
    {
        public HttpStatusCode? StatusCode { get; set; }
        public bool Exception { get; set; }
    }
}