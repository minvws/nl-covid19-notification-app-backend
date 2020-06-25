// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509.Store;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    public class CmsSigner : IContentSigner
    {
        private readonly ICertificateProvider _Provider;

        public CmsSigner(ICertificateProvider provider)
        {
            _Provider = provider;
        }

        public string SignatureOid => "TODO some official OID";

        public byte[] GetSignature(byte[] content)
        {
            var cer = _Provider.GetCertificate();

            if (cer == null)
                throw new InvalidOperationException("Certificate not found.");

            var cert = DotNetUtilities.FromX509Certificate(cer);
            var signaturePair = DotNetUtilities.GetKeyPair(cer.PrivateKey);

            IList certList = new ArrayList();
            IList crlList = new ArrayList();
            certList.Add(cert);

            IX509Store x509Certs = X509StoreFactory.Create("Certificate/Collection", new X509CollectionStoreParameters(certList));
            IX509Store x509Crls = X509StoreFactory.Create("CRL/Collection", new X509CollectionStoreParameters(crlList));

            var gen = new CmsSignedDataGenerator();
            gen.AddCertificates(x509Certs);

            gen.AddSigner(signaturePair.Private, cert, CmsSignedGenerator.DigestSha256);
            //gen.AddCrls(x509Crls);
            
            var cmsSignedData = gen.Generate(new CmsProcessableByteArray(content), false);
            var signature = cmsSignedData.GetEncoded(Asn1Encodable.Der);

            return signature;
        }

        public int LengthBytes => 1623;
    }
}
