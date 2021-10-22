// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class EksJobContentWriterLoggingExtensions
    {
        private const string Name = "EksJobContentWriter";
        private const int Base = LoggingCodex.EksJobContentWriter;
        private const int Published = Base + 1;

        private readonly ILogger _logger;

        public EksJobContentWriterLoggingExtensions(ILogger<EksJobContentWriterLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WritePublished(GaenVersion version, int count)
        {
            _logger.LogInformation($"[{Name}/{Published}] Published Gaen {version} - EKSs - Count:{count}.");
        }
    }
}
