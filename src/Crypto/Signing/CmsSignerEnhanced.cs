// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    public class CmsSignerEnhanced : IContentSigner
    {
        private readonly ICertificateProvider _certificateProvider;
        private readonly ICertificateChainConfig _certificateChainConfig;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IThumbprintConfig _thumbprintConfig;

        public CmsSignerEnhanced(
            ICertificateProvider certificateProvider,
            ICertificateChainConfig certificateConfig,
            IUtcDateTimeProvider dateTimeProvider,
            IThumbprintConfig thumbprintConfig)
        {
            _certificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _certificateChainConfig = certificateConfig ?? throw new ArgumentNullException(nameof(certificateConfig));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _thumbprintConfig = thumbprintConfig ?? throw new ArgumentNullException(nameof(thumbprintConfig));
        }

        public string SignatureOid => "2.16.840.1.101.3.4.2.1";

        public byte[] GetSignature(byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var certificate = _certificateProvider.GetCertificate(_thumbprintConfig.Thumbprint, _thumbprintConfig.RootTrusted);
            var certificateChain = GetCertificateChain();

            var contentInfo = new ContentInfo(content);
            var signedCms = new SignedCms(contentInfo, true);

            signedCms.Certificates.AddRange(certificateChain);

            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            var signingTime = new Pkcs9SigningTime(_dateTimeProvider.Now());
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

        public X509Certificate2[] GetCertificateChain()
        {
            var certList = new List<X509Certificate2>();

            using var s = ResourcesHook.GetManifestResourceStream(_certificateChainConfig.Path);

            if (s == null)
            {
                throw new InvalidOperationException($"Certificate chain not found in resource.");
            }

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            var result = new X509Certificate2Collection();
            result.Import(bytes);
            foreach (var c in result)
            {
                if (c.IssuerName.Name != c.SubjectName.Name)
                {
                    certList.Add(c);
                }
            }

            return certList.ToArray();
        }
    }
}
