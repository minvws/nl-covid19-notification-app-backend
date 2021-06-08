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
        private readonly string _name = "EmbeddedResourceCertificateProvider";
        private const int Base = LoggingCodex.EmbeddedCertProvider;

        private const int Opening = Base + 1;
        private const int ResourceFound = Base + 2;
        private const int ResourceFail = Base + 3;

        private readonly ILogger _logger;

        public EmbeddedCertProviderLoggingExtensions(ILogger<EmbeddedCertProviderLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteOpening(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _logger.LogInformation("[{name}/{id}] Opening resource: {ResName}.",
                _name, Opening,
                name);
        }

        public void WriteResourceFound()
        {
            _logger.LogInformation("[{name}/{id}] Resource found.",
                _name, ResourceFound);
        }

        public void WriteResourceFail()
        {
            _logger.LogError("[{name}/{id}] Failed to get manifest resource stream.",
                _name, ResourceFail);
        }
    }

}
