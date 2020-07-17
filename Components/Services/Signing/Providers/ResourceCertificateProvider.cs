// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    public class StandardCertificateLocationConfig : AppSettingsReader, ICertificateLocationConfig
    {
        public StandardCertificateLocationConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string Path => GetValue(nameof(Path));
        public string Password => GetValue(nameof(Password));
    }

    public interface ICertificateLocationConfig
    {
        public string Path { get; }
        public string Password { get; }
    }

    public class ResourceCertificateProvider3 : ICertificateProvider
    {
        private readonly ICertificateLocationConfig _Config;

        public ResourceCertificateProvider3(ICertificateLocationConfig config)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public X509Certificate2? GetCertificate()
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            using var s = a.GetManifestResourceStream($"NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Resources.{_Config.Path}");

            if (s == null)
                return null;

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            return new X509Certificate2(bytes, _Config.Password, X509KeyStorageFlags.Exportable);
        }
    }

    [Obsolete("Use this class only for testing purposes")]
    public class ResourceCertificateProvider : ICertificateProvider
    {
        private readonly string _ResourceName;

        public ResourceCertificateProvider(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException(nameof(resourceName));

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

    [Obsolete("Use this class only for testing purposes")]
    public class ResourceCertificateProvider2 : ICertificateProvider
    {
        private readonly string _ResourceName;

        public ResourceCertificateProvider2(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException(nameof(resourceName));

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

            return new X509Certificate2(bytes, "", X509KeyStorageFlags.Exportable);
        }
    }
}
