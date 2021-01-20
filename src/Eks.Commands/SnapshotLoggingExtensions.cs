// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class SnapshotLoggingExtensions
    {
        private const string Name = "Snapshot";

        private const int Base = LoggingCodex.Snapshot;
        private const int Start = Base;
        private const int TeksToPublish = Base + 1;

        private readonly ILogger _Logger;

        public SnapshotLoggingExtensions(ILogger<SnapshotLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _Logger.LogDebug("[{name}/{id}] Snapshot publishable TEKs..",
                Name, Start);
        }

        public void WriteTeksToPublish(int tekCount)
        {
            _Logger.LogInformation("[{name}/{id}] TEKs to publish - Count:{Count}.",
                Name, TeksToPublish,
                tekCount);
        }
    }
}