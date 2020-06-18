using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    public class KeySetSigner
    {
        private readonly ICertificateProvider _Provider;
        private const string SignatureAlgorithmDescription = "sha256ECDSA";

        public KeySetSigner(ICertificateProvider provider)
        {
            _Provider = provider;
        }

        public string SignatureDescription => SignatureAlgorithmDescription;

        public byte[] GetSignature(byte[] content)
        {
            using var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(content);

            var cert = _Provider.GetCertificate();

            if (cert == null)
                throw new InvalidOperationException("Certificate not found");

            var signer = cert.GetECDsaPrivateKey();
            return signer.SignHash(hash);
        }
    }
}
