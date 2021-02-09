// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class EmbeddedCertProviderLoggingExtensions
    {
        private string Name = "EmbeddedResourceCertificateProvider";
        private const int Base = LoggingCodex.EmbeddedCertProvider;

        private const int Opening = Base + 1;
        private const int ResourceFound = Base + 2;
        private const int ResourceFail = Base + 3;

        private readonly ILogger _Logger;

        public EmbeddedCertProviderLoggingExtensions(ILogger<EmbeddedCertProviderLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteOpening(string? name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _Logger.LogInformation("[{name}/{id}] Opening resource: {ResName}.",
                Name, Opening,
                name);
        }

        public void WriteResourceFound()
        {
            _Logger.LogInformation("[{name}/{id}] Resource found.",
                Name, ResourceFound);
        }

        public void WriteResourceFail()
        {
            _Logger.LogError("[{name}/{id}] Failed to get manifest resource stream.",
                Name, ResourceFail);
        }
    }

}