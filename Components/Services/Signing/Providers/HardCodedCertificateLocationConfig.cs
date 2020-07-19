using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    public class HardCodedCertificateLocationConfig : ICertificateLocationConfig
    {
        public HardCodedCertificateLocationConfig(string path, string password)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public string Path { get; }
        public string Password { get; }
    }
}