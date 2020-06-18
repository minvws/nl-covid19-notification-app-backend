// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing
{
    public class HsmSigner : ISigner
    {
        private const string SignatureAlgorithmDescription = "SHA256withECDSA";
        private readonly IExposureKeySetSigningConfig _Config;

        public HsmSigner(IExposureKeySetSigningConfig config)
        {
            _Config = config;
        }

        public string SignatureDescription => SignatureAlgorithmDescription;

        public byte[] GetSignature(byte[] content)
        {
            using var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(content);

            var cert = GetCertificate();
            if (cert == null) 
                throw new InvalidOperationException("Certificate not found");
            
            var signer = cert.GetECDsaPrivateKey();  
            return signer.SignHash(hash);
        }

        public int LengthBytes => 64;

        private X509Certificate2 GetCertificate()
        {
            using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            var result = store.Certificates
                .Find(X509FindType.FindByThumbprint, _Config.Thumbprint, false) //TODO Surely true? Setting?
                .OfType<X509Certificate2>()
                .FirstOrDefault();

            return result;
        }
    }
}