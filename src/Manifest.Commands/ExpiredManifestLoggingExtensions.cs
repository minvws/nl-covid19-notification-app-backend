// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class ExpiredManifestLoggingExtensions
    {
        private const string Name = "RemoveExpiredManifest";
        private const int Base = LoggingCodex.RemoveExpiredManifest;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int RemovingManifests = Base + 1;
        private const int RemovingEntry = Base + 2;
        private const int ReconciliationFailed = Base + 3;
        private const int DeletionReconciliationFailed = Base + 4;
        private const int FinishedNothingRemoved = Base + 98;
        
        private readonly ILogger _Logger;

        public ExpiredManifestLoggingExtensions(ILogger<ExpiredManifestLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart(int keepAliveCount)
        {
            _Logger.LogInformation("[{name}/{id}] Begin removing expired Manifests - Keep Alive Count:{count}.",
                Name, Start,
                keepAliveCount);
        }

        public void WriteFinished(int zombieCount, int givenMercyCount)
        {
            _Logger.LogInformation("[{name}/{id}] Finished removing expired Manifests - ExpectedCount:{count} ActualCount:{givenMercy}.",
                Name, Finished,
                zombieCount, givenMercyCount);
        }

        public void WriteFinishedNothingRemoved()
        {
            _Logger.LogInformation("[{name}/{id}] Finished removing expired Manifests - Nothing to remove.",
                Name, FinishedNothingRemoved);
        }

        public void WriteRemovingManifests(int zombieCount)
        {
            _Logger.LogInformation("[{name}/{id}] Removing expired Manifests - Count:{count}.",
                Name, RemovingManifests,
                zombieCount);
        }

        public void WriteRemovingEntry(string publishingId, DateTime releaseDate)
        {
            _Logger.LogInformation("[{name}/{id}] Removing expired Manifest - PublishingId:{PublishingId} Release:{Release}.",
                Name, RemovingEntry,
                publishingId, releaseDate);
        }

        public void WriteReconcilliationFailed(int reconciliationCount)
        {
            _Logger.LogError("[{name}/{id}] Reconciliation failed removing expired Manifests - Found-GivenMercy-Remaining={reconciliation}.",
                Name, ReconciliationFailed,
                reconciliationCount);
        }

        public void WriteDeletionReconciliationFailed(int deleteReconciliationCount)
        {
            _Logger.LogError("[{name}/{id}] Reconciliation failed removing expired Manifests - Zombies-GivenMercy={deadReconciliation}.",
                Name, DeletionReconciliationFailed,
                deleteReconciliationCount);
        }
    }
}
