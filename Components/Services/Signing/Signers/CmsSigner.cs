// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    public class EmbeddedResourcePathConfig : AppSettingsReader, IEmbeddedResourcesPathConfig
    {
        public EmbeddedResourcePathConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string Path => GetConfigValue(nameof(Path), "Missed!");
    }

    public interface IEmbeddedResourcesPathConfig
    {
        string Path { get; }
    }

    public class EmbeddedResourcesCertificateChainProvider : ICertificateChainProvider
    {
        private readonly IEmbeddedResourcesPathConfig _PathProvider;

        public EmbeddedResourcesCertificateChainProvider(IEmbeddedResourcesPathConfig pathProvider)
        {
            _PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        public X509Certificate2[] GetCertificates()
        {
            var certList = new List<X509Certificate2>();
            var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(_PathProvider.Path);

            if (s == null)
                throw new InvalidOperationException($"Certificate chain not found in resources - Path:{_PathProvider.Path}.");

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            var result = new X509Certificate2Collection();
            result.Import(bytes);
            foreach (var c in result)
            {
                if (c.IssuerName.Name != c.SubjectName.Name) //TODO understand this? //Error if cert not required?
                    certList.Add(c);
            }

            return certList.ToArray();
        }
    }

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
                if (e.GetType().Name == "WindowsCryptographicException" && e.Message == "Keyset does not exist" && !WindowsIdentityStuff.CurrentUserIsAdministrator())
                {
                    throw new InvalidOperationException("Failed to sign with certificate when current user does not have UAC elevated permissions.", e);
                }

                throw;
            }

            return signedCms.Encode();
        }
    }
}
