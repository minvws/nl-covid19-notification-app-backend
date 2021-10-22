// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    public abstract class EcdsaBaseSigner
    {
        protected readonly ICertificateProvider Provider;
        protected readonly IThumbprintConfig Config;

        protected EcdsaBaseSigner(ICertificateProvider provider, IThumbprintConfig config)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public virtual byte[] GetSignature(byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var cert = Provider.GetCertificate(Config.Thumbprint, Config.RootTrusted);

            if (cert == null)
            {
                throw new InvalidOperationException("Certificate not found");
            }

            //Should be 70 or so but not fixed length
            //Adds X.962 packaging?
            //Adds 8 magical bytes.
            var notTheResult = cert.GetECDsaPrivateKey().SignData(content, HashAlgorithmName.SHA256);
            return new X962PackagingFix().Format(notTheResult);
        }
    }
}
