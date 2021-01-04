// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class EfgsCmsSigner : IIksSigner
    {
        private readonly ICertificateProvider _CertificateProvider;

        public EfgsCmsSigner(ICertificateProvider certificateProvider)
        {
            _CertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
        }

        public string SignatureOid => "2.16.840.1.101.3.4.2.1";

        public byte[] GetSignature(byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var certificate = _CertificateProvider.GetCertificate();

            if (!certificate.HasPrivateKey)
                throw new InvalidOperationException($"Certificate does not have a private key - Subject:{certificate.Subject} Thumbprint:{certificate.Thumbprint}.");

            var contentInfo = new ContentInfo(content);
            var signedCms = new SignedCms(contentInfo, true);

            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate)
            {
                IncludeOption = X509IncludeOption.EndCertOnly
            };

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