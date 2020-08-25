// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    public class EmbeddedResourceCertificateProvider : ICertificateProvider
    {
        private readonly ICertificateLocationConfig _Config;
        private readonly ILogger _Logger;

        public EmbeddedResourceCertificateProvider(ICertificateLocationConfig config, ILogger<EmbeddedResourceCertificateProvider> logger)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public X509Certificate2 GetCertificate()
        {
            var a = typeof(EmbeddedResourceCertificateProvider).Assembly;

            var resName = $"NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Resources.{_Config.Path}";

            _Logger.LogInformation("Opening resource: {ResName}", resName);

            using var s = GetStream(a, resName);
            if (s == null)
                throw new InvalidOperationException("Could not find resource.");

            _Logger.LogInformation("Resource found.");
            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);
            return new X509Certificate2(bytes, _Config.Password, X509KeyStorageFlags.Exportable);
        }

        private Stream GetStream(Assembly a, string resName)
        {
            try
            {
                return a.GetManifestResourceStream(resName);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Failed to get manifest resource stream");
                throw;
            }
        }
    }
}