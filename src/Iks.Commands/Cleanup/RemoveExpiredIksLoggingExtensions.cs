// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup
{
    public class RemoveExpiredIksLoggingExtensions
    {
        private const string Name = "RemoveExpiredIks";
        private const int Base = LoggingCodex.RemoveExpiredIks;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int CurrentIksFound = Base + 1;
        private const int FoundTotal = Base + 2;
        private const int FoundEntry = Base + 3;
        private const int RemovedAmount = Base + 4;
        private const int ReconciliationFailed = Base + 5;
        private const int FinishedNothingRemoved = Base + 98;

        private readonly ILogger _logger;
        private string _iksType = "IKS"; // IksIn or IksOut, defaults to just IKS

        public RemoveExpiredIksLoggingExtensions(ILogger<RemoveExpiredIksLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart(string iksType)
        {
            _iksType = iksType;
            _logger.LogInformation($"[{Name}/{Start}] Begin removing expired {_iksType}.");
        }

        public void WriteFinished()
        {
            _logger.LogInformation($"[{Name}/{Finished}] Finished {_iksType} cleanup.");
        }

        public void WriteFinishedNothingRemoved()
        {
            _logger.LogInformation($"[{Name}/{FinishedNothingRemoved}] Finished {_iksType} cleanup. In safe mode - no deletions.");
        }

        public void WriteCurrentIksFound(int totalFound)
        {
            _logger.LogInformation($"[{Name}/{CurrentIksFound}] Current {_iksType} - Count:{totalFound}.");
        }

        public void WriteTotalIksFound(DateTime cutoff, int zombiesFound)
        {
            _logger.LogInformation($"[{Name}/{FoundTotal}] Found expired {_iksType} - Cutoff:{cutoff:yyyy-MM-dd}, Count:{zombiesFound}");
        }

        public void WriteEntryFound(string publishingId, DateTime releaseDate)
        {
            _logger.LogInformation($"[{Name}/{FoundEntry}] Found expired {_iksType} - PublishingId:{publishingId} Release:{releaseDate}");
        }

        public void WriteRemovedAmount(int givenMercy, int remaining)
        {
            _logger.LogInformation($"[{Name}/{RemovedAmount}] Removed expired {_iksType} - Count:{givenMercy}, Remaining:{remaining}");
        }

        public void WriteReconciliationFailed(int reconciliationCount)
        {
            _logger.LogError($"[{Name}/{ReconciliationFailed}] Reconciliation failed - Found-GivenMercy-Remaining:{reconciliationCount}.");
        }
    }
}
