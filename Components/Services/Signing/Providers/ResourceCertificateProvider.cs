// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    [Obsolete("Use this class only for testing purposes")]
    public class ResourceCertificateProvider : ICertificateProvider
    {
        private readonly string _ResourceName;

        public ResourceCertificateProvider(string resourceName)
        {
            _ResourceName = resourceName;
        }

        public X509Certificate2? GetCertificate()
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            using var s = a.GetManifestResourceStream($"NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Resources.{_ResourceName}");
            if (s == null)
                return null;

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            return new X509Certificate2(bytes, "Covid-19!", X509KeyStorageFlags.Exportable);
        }
    }
}
