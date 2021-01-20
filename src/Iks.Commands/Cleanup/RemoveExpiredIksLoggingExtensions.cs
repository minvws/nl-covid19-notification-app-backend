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

        private readonly ILogger _Logger;
        private string IksType = "IKS"; // IksIn or IksOut, defaults to just IKS

        public RemoveExpiredIksLoggingExtensions(ILogger<RemoveExpiredIksLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart(string iksType)
        {
            IksType = iksType;
            _Logger.LogInformation($"[{Name}/{Start}] Begin removing expired {IksType}.");
        }

        public void WriteFinished()
        {
            _Logger.LogInformation($"[{Name}/{Finished}] Finished {IksType} cleanup.");
        }

        public void WriteFinishedNothingRemoved()
        {
            _Logger.LogInformation($"[{Name}/{FinishedNothingRemoved}] Finished {IksType} cleanup. In safe mode - no deletions.");
        }

        public void WriteCurrentIksFound(int totalFound)
        {
            _Logger.LogInformation($"[{Name}/{CurrentIksFound}] Current {IksType} - Count:{totalFound}.");
        }

        public void WriteTotalIksFound(DateTime cutoff, int zombiesFound)
        {
            _Logger.LogInformation($"[{Name}/{FoundTotal}] Found expired {IksType} - Cutoff:{cutoff:yyyy-MM-dd}, Count:{zombiesFound}");
        }

        public void WriteEntryFound(string publishingId, DateTime releaseDate)
        {
            _Logger.LogInformation($"[{Name}/{FoundEntry}] Found expired {IksType} - PublishingId:{publishingId} Release:{releaseDate}");
        }

        public void WriteRemovedAmount(int givenMercy, int remaining)
        {
            _Logger.LogInformation($"[{Name}/{RemovedAmount}] Removed expired {IksType} - Count:{givenMercy}, Remaining:{remaining}");
        }

        public void WriteReconciliationFailed(int reconciliationCount)
        {
            _Logger.LogError($"[{Name}/{ReconciliationFailed}] Reconciliation failed - Found-GivenMercy-Remaining:{reconciliationCount}.");
        }
    }
}
