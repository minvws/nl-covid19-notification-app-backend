using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    public class HSMCertificateProvider : ICertificateProvider
    {
        private readonly string _Thumbprint;

        public HSMCertificateProvider(string thumbprint)
        {
            _Thumbprint = thumbprint;
        }

        public X509Certificate2? GetCertificate()
        {
            using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            var result = store.Certificates
                .Find(X509FindType.FindByThumbprint, _Thumbprint, false) //TODO Surely true? Setting?
                .OfType<X509Certificate2>()
                .FirstOrDefault();

            return result;
        }
    }
}
