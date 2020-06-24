// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    /// <summary>
    /// For GAEN EKS Signing
    /// </summary>
    public class EcdSaSigner : IContentSigner
    {
        private readonly ICertificateProvider _Provider;
        private const string SignatureAlgorithmDescription = "1.2.840.10045.4.3.2";

        public EcdSaSigner(ICertificateProvider provider)
        {
            _Provider = provider;
        }

        public string SignatureOid => SignatureAlgorithmDescription;

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

        public int LengthBytes => 32;

    }
}
