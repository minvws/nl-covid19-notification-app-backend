// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Security;

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
            var cert = _Provider.GetCertificate();

            if (cert == null)
                throw new InvalidOperationException("Certificate not found.");

            var key = DotNetUtilities.GetKeyPair(cert.PrivateKey).Private;
            var gen = new CmsSignedDataGenerator();
            gen.AddSigner(key, DotNetUtilities.FromX509Certificate(cert), CmsSignedGenerator.DigestSha256);
            var cmsSignedData = gen.Generate(new CmsProcessableByteArray(content));
            var signature = cmsSignedData.GetEncoded(Asn1Encodable.Der);
            return signature;
        }

        public int LengthBytes => 620;
    }
}
