// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    /// <summary>
    /// For GAEN EKS Signing
    /// </summary>
    public class EcdSaSigner : IGaContentSigner
    {
        private readonly ICertificateProvider _Provider;
        private const string SignatureAlgorithmDescription = "1.2.840.10045.4.3.2";

        public EcdSaSigner(ICertificateProvider provider)
        {
            _Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public string SignatureOid => SignatureAlgorithmDescription;

        public byte[] GetSignature(byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            //using var hasher = SHA256.Create();
            //var hash = hasher.ComputeHash(content);

            var cert = _Provider.GetCertificate();

            if (cert == null)
                throw new InvalidOperationException("Certificate not found");

            //Should be 70 or so but not fixed length
            //Adds X.962 packaging?
            //Adds 8 magical bytes.
            var notTheResult = cert.GetECDsaPrivateKey().SignData(content, HashAlgorithmName.SHA256);
            return new X962PackagingFix().Format(notTheResult);
        }
    }
}
