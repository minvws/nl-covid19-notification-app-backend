// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsDownloader.Jobs
{
    public class IksPollingBatchJob : IJob
    {
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IHttpGetIksCommand _receiver;
        private readonly IIksWriterCommand _writer;
        private readonly IksInDbContext _iksInDbContext;
        private readonly IEfgsConfig _efgsConfig;
        private readonly IksDownloaderLoggingExtensions _logger;

        public IksPollingBatchJob(
            IUtcDateTimeProvider dateTimeProvider,
            IHttpGetIksCommand receiver,
            IIksWriterCommand writer,
            IksInDbContext iksInDbContext,
            IEfgsConfig efgsConfig,
            IksDownloaderLoggingExtensions logger)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _iksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _efgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Run()
        {
            if (!_efgsConfig.DownloaderEnabled)
            {
                _logger.WriteDisabledByConfig();
                return;
            }

            var jobInfo = GetJobInfo();
            var lastWrittenBatchTag = jobInfo.LastBatchTag;
            var tagToRequest = string.Empty;

            // Set date to a default of today.
            // Either we continue where we left off, or we grab as many batches as allowed
            // (i.e., a few days worth back from today)
            var dayToRequest = _dateTimeProvider.Snapshot.Date;

            var batchTagDatePart = lastWrittenBatchTag.Split("-").FirstOrDefault();

            if (!string.IsNullOrEmpty(batchTagDatePart))
            {
                // All is good, start requesting batch from where we left off.
                // The date and the batchTag's creation date need to be the same,
                // otherwise EFGS will return a HTTP status 404.
                dayToRequest = DateTime.ParseExact(batchTagDatePart, "yyyyMMdd", null);
                tagToRequest = lastWrittenBatchTag;
            }
            else
            {
                // If lastWrittenBatchTag is somehow unusable or unavailable,
                // go as far back as allowed with the date and don't send a batchTag to EFGS
                // (i.e., keep tagToRequest as an empty string)
                dayToRequest = dayToRequest.AddDays(_efgsConfig.DaysToDownload * -1);
            }

            var downloadCount = 0;
            var daysLeft = true;

            while (daysLeft && downloadCount <= _efgsConfig.MaxBatchesPerRun)
            {
                var result = _receiver.ExecuteAsync(dayToRequest, tagToRequest).GetAwaiter().GetResult();
                downloadCount++;

                // If we have a success response and we haven't already received the current batchTag, process it
                if (result != null && !_iksInDbContext.Received.AsNoTracking().Any(x => x.BatchTag == result.BatchTag))
                {
                    _logger.WriteProcessingData(dayToRequest, result.BatchTag);

                    try
                    {
                        // Persists batch to database
                        _writer.Execute(new IksWriteArgs
                        {
                            BatchTag = result.BatchTag,
                            Content = result.Content
                        });

                        // Update jobinfo:
                        // Keep track of the last batch we wrote to the database
                        jobInfo.LastBatchTag = result.BatchTag;
                        // And keep track of the last time this job was run in a functionally meaningful way
                        jobInfo.LastRun = _dateTimeProvider.Snapshot;

                        // Persist updated jobinfo to database
                        _iksInDbContext.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        _logger.WriteEfgsError(e);
                    }
                }
                else if (result != null)
                {
                    // Log this for informational purposes
                    _logger.WriteBatchAlreadyProcessed(result.BatchTag);
                }

                // Move on to the next batchTag if available; otherwise, move on to the next day.
                // If result was null because of an non-success response from EFGS,
                // it will be dealt with here by moving on to a next day if possible.
                tagToRequest = result?.NextBatchTag ?? string.Empty;

                if (string.IsNullOrEmpty(tagToRequest))
                {
                    _logger.WriteNoNextBatch();

                    if (dayToRequest < _dateTimeProvider.Snapshot.Date)
                    {
                        _logger.WriteMovingToNextDay();
                        dayToRequest = dayToRequest.AddDays(1);
                    }
                    else
                    {
                        _logger.WriteNoNextBatchNoMoreDays();
                        daysLeft = false;
                    }
                }
                else
                {
                    // Log this for informational purposes
                    _logger.WriteNextBatchFound(tagToRequest);
                    _logger.WriteBatchProcessedInNextLoop(tagToRequest);
                }

                // Log this for informational purposes
                if (downloadCount > _efgsConfig.MaxBatchesPerRun)
                {
                    _logger.WriteBatchMaximumReached(_efgsConfig.MaxBatchesPerRun);
                }
            }
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
