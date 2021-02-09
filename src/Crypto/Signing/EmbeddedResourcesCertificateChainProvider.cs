// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    public class EmbeddedResourcesCertificateChainProvider : ICertificateChainProvider
    {
        private readonly IEmbeddedResourceCertificateConfig _PathProvider;

        public EmbeddedResourcesCertificateChainProvider(IEmbeddedResourceCertificateConfig pathProvider)
        {
            _PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        public X509Certificate2[] GetCertificates()
        {
            var certList = new List<X509Certificate2>();

            using var s = ResourcesHook.GetManifestResourceStream(_PathProvider.Path);

            if (s == null)
                throw new InvalidOperationException($"Certificate chain not found in resource.");

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            var result = new X509Certificate2Collection();
            result.Import(bytes);
            foreach (var c in result)
            {
                if (c.IssuerName.Name != c.SubjectName.Name)
                    certList.Add(c);
            }

            return certList.ToArray();
        }
    }
}