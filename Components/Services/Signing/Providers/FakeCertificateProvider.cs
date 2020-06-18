// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    [Obsolete("Use this class only for testing purposes")]
    public class FakeCertificateProvider : ICertificateProvider
    {
        private readonly string _ResourceName;

        public FakeCertificateProvider(string resourceName)
        {
            _ResourceName = resourceName;
        }

        public X509Certificate2? GetCertificate()
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using Stream s = a.GetManifestResourceStream($"NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Resources.{_ResourceName}");
            if (s == null)
                return null;

            byte[] bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            return new X509Certificate2(bytes, "Covid-19!", X509KeyStorageFlags.Exportable);
        }
    }
}
