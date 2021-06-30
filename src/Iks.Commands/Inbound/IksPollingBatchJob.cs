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
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly Func<IHttpGetIksCommand> _receiverFactory;
        private readonly Func<IIksWriterCommand> _writerFactory;
        private readonly IksInDbContext _iksInDbContext;
        private readonly IEfgsConfig _efgsConfig;
        private readonly IksDownloaderLoggingExtensions _logger;

        public IksPollingBatchJob(
            IUtcDateTimeProvider dateTimeProvider,
            Func<IHttpGetIksCommand> receiverFactory,
            Func<IIksWriterCommand> writerFactory,
            IksInDbContext iksInDbContext,
            IEfgsConfig efgsConfig,
            IksDownloaderLoggingExtensions logger)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _receiverFactory = receiverFactory ?? throw new ArgumentNullException(nameof(receiverFactory));
            _writerFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));
            _iksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _efgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync()
        {
            var jobInfo = GetJobInfo();
            var downloadCount = 0;
            var lastWrittenBatchTag = jobInfo.LastBatchTag;

            // Set date to a default of today.
            // Either we continue where we left off, or we grab as many batches as allowed
            // (i.e., a few days worth back from today)
            var date = _dateTimeProvider.Snapshot.Date;
            var batchTagDatePart = lastWrittenBatchTag.Split("-").FirstOrDefault();

            if (!string.IsNullOrEmpty(batchTagDatePart))
            {
                // All is good, start requesting batch from where we left off.
                // The date and the batchTag's creation date need to be the same,
                // otherwise EFGS will return a HTTP status 404.
                date = DateTime.ParseExact(batchTagDatePart, "yyyyMMdd", null);
            }
            else
            {
                // If lastWrittenBatchTag is somehow unusable or unavailable,
                // go as far back as allowed and don't send a batchTag to EFGS.
                date = date.AddDays(_efgsConfig.DaysToDownload * -1);
                lastWrittenBatchTag = string.Empty;
            }

            // If we have a batchTag, we will re-request that batch from EFGS to start our run.

            // If we do not have a batchTag (it is null or empty), the first batch of the date requested will be returned by EFGS.
            // We may already have some of that requested date's batches,
            // but we may not have all its batches or batches from the subsequent days.

            var result = await _receiverFactory().ExecuteAsync(date, lastWrittenBatchTag);
            downloadCount++;

            while (result != null && downloadCount <= _efgsConfig.MaxBatchesPerRun)
            {
                // If we haven't already received the current batchTag, process it
                if (!_iksInDbContext.Received.Any(x => x.BatchTag == result.BatchTag))
                {
                    _logger.WriteProcessingData(date, result.BatchTag);

                    try
                    {
                        await WriteSingleBatchAsync(result.BatchTag, result.Content);
                        lastWrittenBatchTag = result.BatchTag;
                        await UpdateJobInfoAsync(jobInfo, lastWrittenBatchTag);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteEfgsError(e);
                    }
                }
                else
                {
                    _logger.WriteBatchAlreadyProcessed(result.BatchTag);
                }

                // Move on to the next batchTag
                if (!string.IsNullOrEmpty(result.NextBatchTag))
                {
                    _logger.WriteNextBatchFound(result.NextBatchTag);
                    _logger.WriteBatchProcessedInNextLoop(result.NextBatchTag);

                    result = await _receiverFactory().ExecuteAsync(date, result.NextBatchTag);
                    downloadCount++;
                }
                else
                {
                    // No next batch available, we're done for lastWrittenBatchTag's day's set of batches.
                    _logger.WriteNoNextBatch();

                    // Check if we can move on to a possible next day's worth of batches,
                    // now that this current set of batches is finished.
                    // Don't move past today though :)
                    if (date < _dateTimeProvider.Snapshot.Date)
                    {
                        _logger.WriteMovingToNextDay();
                        date = date.AddDays(1);
                        result = await _receiverFactory().ExecuteAsync(date, string.Empty);
                        downloadCount++;
                    }
                    else
                    {
                        // No more days with batches available, we're done.
                        _logger.WriteNoNextBatchNoMoreDays();
                        result = null;
                    }
                }

                // Log this for informational purposes
                if (downloadCount > _efgsConfig.MaxBatchesPerRun)
                {
                    _logger.WriteBatchMaximumReached(downloadCount);
                }
            }
        }

        private async Task UpdateJobInfoAsync(IksInJobEntity jobInfo, string lastWrittenBatchTag)
        {
            // Keep track of the last batch we wrote to the database
            jobInfo.LastBatchTag = lastWrittenBatchTag;
            // And keep track of the last time this job was run
            jobInfo.LastRun = _dateTimeProvider.Snapshot.Date;

            // Persist updated jobinfo to database.
            await _iksInDbContext.SaveChangesAsync();
        }

        private async Task WriteSingleBatchAsync(string batchTag, byte[] content)
        {
            var writer = _writerFactory();

            await writer.Execute(new IksWriteArgs
            {
                BatchTag = batchTag,
                Content = content
            });

            // Persist batch to database.
            await _iksInDbContext.SaveChangesAsync();
        }

        private IksInJobEntity GetJobInfo()
        {
            var result = _iksInDbContext.InJob.SingleOrDefault();

            if (result != null)
            {
                return result;
            }

            result = new IksInJobEntity();
            _iksInDbContext.InJob.Add(result);
            return result;
        }
    }
}
