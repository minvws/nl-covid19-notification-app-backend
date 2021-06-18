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
            var today = _dateTimeProvider.Snapshot.Date;

            // Fetch all available batches starting from the last received batch we have.
            // There is actually no need to proivde a date if you send a batchTag, but let's do it anyway.
            await FetchAvailableBatchesAsync(jobInfo.LastBatchTag, today);

            // Find the last succesfully received batchTag
            var lastBatchTag = _iksInDbContext.Received
                .Where(x => x.Error != true)
                .OrderByDescending(x => x.Created)
                .Select(x => x.BatchTag)
                .Single();  // If there are no entries without errors, crash for your life!

            // Update IksInJob
            jobInfo.LastBatchTag = lastBatchTag;
            jobInfo.LastRun = today;

            // Persist updated jobinfo to database.
            await _iksInDbContext.SaveChangesAsync();
        }

        private async Task FetchAvailableBatchesAsync(string currentBatchTag, DateTime date, int count = 0)
        {
            if (count == _efgsConfig.MaxBatchesPerRun)
            {
                // Maximum number of batches reached, stop.
                return;
            }

            // Request the currentBatch, and any possible nextBatchTag values for subsequent processing
            var result = await _receiverFactory().ExecuteAsync(currentBatchTag, date);

            if (result == null)
            {
                // No love from EFGS, stop this run and try again next time.
                return;
            }

            // If we have already previously received the currentBatchTag, log it
            // and move on to the nextBatchTag, or stop.
            if (_iksInDbContext.Received.Any(x => x.BatchTag == result.BatchTag))
            {
                _logger.WriteBatchAlreadyProcessed(result.BatchTag);

                if (result.NextBatchTag == null)
                {
                    // No next batch available, we are done here.
                    return;
                }

                // Fetch the next batch, and increase our "download count"
                await FetchAvailableBatchesAsync(currentBatchTag: result.NextBatchTag, date: date, count: count++);
            }

            // If we haven't already received the currentBatchTag, process it
            await WriteSingleBatchAsync(currentBatchTag, result.Content);

            // Check if we have a next batch available for us
            if (result.NextBatchTag != null)
            {
                // Fetch the next batch
                await FetchAvailableBatchesAsync(currentBatchTag: result.NextBatchTag, date: date, count: count++);
            }

            // No next batch available, we are done here.
            return;
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
