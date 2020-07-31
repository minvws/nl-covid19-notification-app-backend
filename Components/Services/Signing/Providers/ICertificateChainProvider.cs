using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    public interface ICertificateChainProvider
    {
        X509Certificate2[] GetCertificates();
    }
}