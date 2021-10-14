// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Eks
{
    public class ExpiredEksV3LoggingExtensions
    {
        private const string Name = "RemoveExpiredEksV3";
        private const int Base = LoggingCodex.RemoveExpiredEksV3;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int CurrentEksFound = Base + 1;
        private const int FoundTotal = Base + 2;
        private const int FoundEntry = Base + 3;
        private const int RemovedAmount = Base + 4;
        private const int ReconciliationFailed = Base + 5;
        private const int FinishedNothingRemoved = Base + 98;

        private readonly ILogger _logger;

        public ExpiredEksV3LoggingExtensions(ILogger<ExpiredEksV3LoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _logger.LogInformation("[{name}/{id}] Begin removing expired EKSv3.",
                Name, Start);
        }

        public void WriteFinished()
        {
            _logger.LogInformation("[{name}/{id}] Finished EKSv3 cleanup.",
                Name, Finished);
        }

        public void WriteFinishedNothingRemoved()
        {
            _logger.LogInformation("[{name}/{id}] Finished EKSv3 cleanup. In safe mode - no deletions.",
                Name, FinishedNothingRemoved);
        }

        public void WriteCurrentEksFound(int totalFound)
        {
            _logger.LogInformation("[{name}/{id}] Current EKS - Count:{found}.",
                Name, CurrentEksFound,
                totalFound);
        }

        public void WriteTotalEksFound(DateTime cutoff, int zombiesFound)
        {
            _logger.LogInformation("[{name}/{id}] Found expired EKSv3 - Cutoff:{cutoff:yyyy-MM-dd}, Count:{count}",
                Name, FoundTotal,
                cutoff, zombiesFound);
        }

        public void WriteEntryFound(string publishingId, DateTime releaseDate)
        {
            _logger.LogInformation("[{name}/{id}] Found expired EKSv3 - PublishingId:{PublishingId} Release:{Release}",
                Name, FoundEntry,
                publishingId, releaseDate);
        }

        public void WriteRemovedAmount(int givenMercy, int remaining)
        {
            _logger.LogInformation("[{name}/{id}] Removed expired EKSv3 - Count:{count}, Remaining:{remaining}",
                Name, RemovedAmount,
                givenMercy, remaining);
        }

        public void WriteReconciliationFailed(int reconciliationCount)
        {
            _logger.LogError("[{name}/{id}] Reconciliation failed - Found-GivenMercy-Remaining:{remaining}.",
                Name, ReconciliationFailed,
                reconciliationCount);
        }
    }
}
