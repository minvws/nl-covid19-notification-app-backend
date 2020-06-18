// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    public class ContentSigner : ISigner
    {
        private readonly ICertificateProvider _Provider;
        
        public ContentSigner(ICertificateProvider provider)
        {
            _Provider = provider;
        }

        public string SignatureDescription => "sha256RSA";

        public byte[] GetSignature(byte[] content)
        {
            using var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(content);

            var cert = _Provider.GetCertificate();

            if (cert == null)
                throw new InvalidOperationException("Certificate not found");

            var signer = cert.GetRSAPrivateKey();
            var signature = signer.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return signature;
        }

        public int LengthBytes => 512;
    }
}
