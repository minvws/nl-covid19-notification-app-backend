// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public class IksPollingBatchJob
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly Func<IIHttpGetIksCommand> _ReceiverFactory;
        private readonly Func<IIksWriterCommand> _WriterFactory;
        private readonly IksInDbContext _IksInDbContext;
        private readonly IEfgsConfig _EfgsConfig;
        private readonly IksDownloaderLoggingExtensions _Logger;

        public IksPollingBatchJob(
            IUtcDateTimeProvider dateTimeProvider,
            Func<IIHttpGetIksCommand> receiverFactory,
            Func<IIksWriterCommand> writerFactory,
            IksInDbContext iksInDbContext, 
            IEfgsConfig efgsConfig,
            IksDownloaderLoggingExtensions logger)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ReceiverFactory = receiverFactory ?? throw new ArgumentNullException(nameof(receiverFactory));
            _WriterFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));
            _IksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _EfgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync()
        {
            var downloadCount = 0;
            var jobInfo = GetJobInfo();
            var batchDate = jobInfo.LastRun;
            var previousBatch = jobInfo.LastBatchTag;
            var batch = jobInfo.LastBatchTag;

            // All of the values on first run are empty, all we need to do is set the date to the first date and we can start
            if (jobInfo.LastRun == DateTime.MinValue)
            {
                batchDate = _DateTimeProvider.Snapshot.Date.AddDays(_EfgsConfig.DaysToDownload * -1);
            }

            // TODO: save JobInfo for each loop and refresh it for each loop. Remove the other variables.
            while (true)
            {
                downloadCount++;

                if (downloadCount == _EfgsConfig.MaxBatchesPerRun)
                {
                    _Logger.WriteBatchMaximumReached(_EfgsConfig.MaxBatchesPerRun);
                    break;
                }

                var result = await _ReceiverFactory().ExecuteAsync(batch, batchDate);

                // Handle no found / gone
                if (result == null)
                {
                    // Move to the next day if possible
                    if (batchDate < _DateTimeProvider.Snapshot.Date)
                    {
                        batchDate = batchDate.AddDays(1);
                        batch = string.Empty;

                        continue;
                    }

                    break;
                }

                _Logger.WriteNextBatchReceived(result.BatchTag, result.NextBatchTag);

                // Process a previous batch
                if (result.BatchTag == previousBatch || _IksInDbContext.Received.Any(_ => _.BatchTag == batch))
                {
                    _Logger.WriteBatchAlreadyProcessed(result.BatchTag);

                    // New batch so process it next time
                    if (!string.IsNullOrWhiteSpace(result.NextBatchTag))
                    {
                        _Logger.WriteBatchProcessedInNextLoop(result.NextBatchTag);

                        batch = result.NextBatchTag;
                    }
                }
                else
                {
                    // Process a new batch
                    var writer = _WriterFactory();

                    await writer.Execute(new IksWriteArgs
                    {
                        BatchTag = result.BatchTag,
                        Content = result.Content
                    });

                    // Update the job info as a batch has been downloaded
                    jobInfo.LastRun = batchDate;
                    jobInfo.LastBatchTag =  result.BatchTag;

                    previousBatch = result.BatchTag;

                    // Move to the next batch if we have one
                    if (!string.IsNullOrWhiteSpace(result.NextBatchTag))
                    {
                        batch = result.NextBatchTag;
                    }
                }

                // No next batch, move to the next date or stop
                if (string.IsNullOrWhiteSpace(result.NextBatchTag))
                {
                    // Move to the next day if possible
                    if (batchDate < _DateTimeProvider.Snapshot.Date)
                    {
                        batchDate = batchDate.AddDays(1);
                        batch = string.Empty;

                        _Logger.WriteMovingToNextDay();

                        continue;
                    }

                    _Logger.WriteNoNextBatch();
                    break;
                }

                _Logger.WriteNextBatchFound(result.NextBatchTag);
            }

            // TODO the downloaded batches must also be done in one transaction
            await _IksInDbContext.SaveChangesAsync();
        }

        private IksInJobEntity GetJobInfo()
        {
            var result = _IksInDbContext.InJob.SingleOrDefault();
            
            if (result != null)
                return result;

            result = new IksInJobEntity();
            _IksInDbContext.InJob.Add(result);
            return result;
        }
    }
}