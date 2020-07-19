using System;
using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    public class AzureResourceCertificateProvider : ICertificateProvider
    {
        private readonly ICertificateLocationConfig _Config;

        public AzureResourceCertificateProvider(ICertificateLocationConfig config)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public X509Certificate2 GetCertificate()
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            using var s = a.GetManifestResourceStream($"NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Resources.{_Config.Path}");

            if (s == null)
                throw new InvalidOperationException("Resource not found.");

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            return new X509Certificate2(bytes, _Config.Password, 
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet |
                X509KeyStorageFlags.Exportable 
            );
        }
    }
}