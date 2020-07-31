using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    public class EmbeddedResourcesCertificateChainProvider : ICertificateChainProvider
    {
        private readonly IEmbeddedResourcesPathConfig _PathProvider;

        public EmbeddedResourcesCertificateChainProvider(IEmbeddedResourcesPathConfig pathProvider)
        {
            _PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        public X509Certificate2[] GetCertificates()
        {
            var certList = new List<X509Certificate2>();
            var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(_PathProvider.Path);

            if (s == null)
                throw new InvalidOperationException($"Certificate chain not found in resources - Path:{_PathProvider.Path}.");

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            var result = new X509Certificate2Collection();
            result.Import(bytes);
            foreach (var c in result)
            {
                if (c.IssuerName.Name != c.SubjectName.Name) //TODO understand this? //Error if cert not required?
                    certList.Add(c);
            }

            return certList.ToArray();
        }
    }
}