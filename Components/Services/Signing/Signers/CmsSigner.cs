// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    public class CmsSigner : IContentSigner
    {
        private readonly ICertificateProvider _Provider;

        public CmsSigner(ICertificateProvider provider)
        {
            _Provider = provider;
        }

        public string SignatureOid => "2.16.840.1.101.3.4.2.1";

        private static X509Certificate2[] GetChainWithoutRoot()
        {
            var certList = new List<X509Certificate2>();

            var a = System.Reflection.Assembly.GetExecutingAssembly();
            using var s = a.GetManifestResourceStream("NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Resources.certs.p7b");
            if (s == null)
                return certList.ToArray();

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            var chain = new X509Certificate2Collection();
            chain.Import(bytes);

            foreach (var c in chain)
            {
                if (c.IssuerName.Name != c.SubjectName.Name)
                    certList.Add(c);
            }

            return certList.ToArray();
        }

        public byte[] GetSignature(byte[] content)
        {
            var certificate = _Provider.GetCertificate();

            if (certificate == null)
                throw new InvalidOperationException("Certificate not found.");

            var chain = GetChainWithoutRoot();

            ContentInfo contentInfo = new ContentInfo(content);
            SignedCms signedCms = new SignedCms(contentInfo, true);
            var signer = new System.Security.Cryptography.Pkcs.CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            signedCms.Certificates.AddRange(chain);
            signedCms.ComputeSignature(signer);

            return signedCms.Encode();
        }

        public int LengthBytes => 1510;
    }
}
