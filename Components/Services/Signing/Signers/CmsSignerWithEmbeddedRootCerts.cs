// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    public class CmsSignerWithEmbeddedRootCerts : IContentSigner
    {
        private readonly ICertificateProvider _Provider;

        public CmsSignerWithEmbeddedRootCerts(ICertificateProvider provider)
        {
            _Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public string SignatureOid => "2.16.840.1.101.3.4.2.1";

        private static X509Certificate2[] GetChainWithoutRoot()
        {
            var certList = new List<X509Certificate2>();

            var a = System.Reflection.Assembly.GetExecutingAssembly();
            using var s = a.GetManifestResourceStream("NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Resources.BdCertChain.p7b");

            if (s == null)
            {
                throw new InvalidOperationException("Root certificates not found - resource certs.p7b missing.");
            }

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
            if (content == null) throw new ArgumentNullException(nameof(content));

            var certificate = _Provider.GetCertificate();

            if (!certificate.HasPrivateKey)
                throw new InvalidOperationException($"Certificate does not have a private key - Subject:{certificate.Subject} Thumbprint:{certificate.Thumbprint}.");

            var chain = GetChainWithoutRoot();

            var contentInfo = new ContentInfo(content);
            var signedCms = new SignedCms(contentInfo, true);
            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            signedCms.Certificates.AddRange(chain);

            try
            {
                signedCms.ComputeSignature(signer);
            }
            catch (Exception e)
            {
                //NB. Cannot catch the internal exception type (cross-platform design of .NET Core)
                if (e.GetType().Name == "WindowsCryptographicException" && e.Message == "Keyset does not exist" && !WindowsIdentityStuff.CurrentUserIsAdministrator())
                {
                    throw new InvalidOperationException("Failed to use certificate when current user does not have UAC elevated permissions.", e);
                }

                throw;
            }

            return signedCms.Encode();
        }
    }
}
