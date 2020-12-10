// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class EksJobContentWriterLoggingExtensions
    {
        private const string Name = "EksJobContentWriter";
        private const int Base = LoggingCodex.EksJobContentWriter;
        private const int Published = Base + 1;

        private readonly ILogger _Logger;

        public EksJobContentWriterLoggingExtensions(ILogger<EksJobContentWriterLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WritePublished(int count)
        {
            _Logger.LogInformation("[{name}/{id}] Published EKSs - Count:{Count}.",
                Name, Published,
                count);
        }
    }
}
