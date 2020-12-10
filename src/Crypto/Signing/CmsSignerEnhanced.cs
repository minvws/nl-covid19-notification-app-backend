// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    public class CmsSignerEnhanced : IContentSigner
    {
        private readonly ICertificateProvider _CertificateProvider;
        private readonly ICertificateChainProvider _CertificateChainProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public CmsSignerEnhanced(ICertificateProvider certificateProvider, ICertificateChainProvider certificateChainProvider, IUtcDateTimeProvider dateTimeProvider)
        {
            _CertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _CertificateChainProvider = certificateChainProvider ?? throw new ArgumentNullException(nameof(certificateChainProvider));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public string SignatureOid => "2.16.840.1.101.3.4.2.1";

        public byte[] GetSignature(byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var certificate = _CertificateProvider.GetCertificate();

            if (!certificate.HasPrivateKey)
                throw new InvalidOperationException($"Certificate does not have a private key - Subject:{certificate.Subject} Thumbprint:{certificate.Thumbprint}.");

            var certificateChain = _CertificateChainProvider.GetCertificates();

            var contentInfo = new ContentInfo(content);
            var signedCms = new SignedCms(contentInfo, true);

            signedCms.Certificates.AddRange(certificateChain);

            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            var signingTime = new Pkcs9SigningTime(_DateTimeProvider.Now());
            signer.SignedAttributes.Add(new CryptographicAttributeObject(signingTime.Oid, new AsnEncodedDataCollection(signingTime)));

            try
            {
                signedCms.ComputeSignature(signer);
            }
            catch (Exception e)
            {
                //NB. Cannot catch the internal exception type (cross-platform design of .NET Core)
                if (e.GetType().Name == "WindowsCryptographicException" && e.Message == "Keyset does not exist" && !WindowsIdentityQueries.CurrentUserIsAdministrator())
                {
                    throw new InvalidOperationException("Failed to sign with certificate when current user does not have UAC elevated permissions.", e);
                }

                throw;
            }

            return signedCms.Encode();
        }
    }
}