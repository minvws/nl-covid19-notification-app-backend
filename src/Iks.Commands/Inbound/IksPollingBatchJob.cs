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

            var date = _dateTimeProvider.Snapshot.Date;
            var batchTagDatePart = lastWrittenBatchTag.Split("-").FirstOrDefault();

            if (!string.IsNullOrEmpty(batchTagDatePart))
            {
                // All is good, start requesting batch from where we left off.
                date = DateTime.ParseExact(batchTagDatePart, "yyyyMMdd", null);
            }
            else
            {
                // If lastWrittenBatchTag is somehow unusable, go as far back as allowed,
                // and don't send the batchTag to EFGS.
                date = date.AddDays(_efgsConfig.DaysToDownload * -1);
                lastWrittenBatchTag = string.Empty;
            }

            // If we have a batchTag, we will request that batch from EFGS.
            // The date parameter will then be used by EFGS to compare against the "created" date of the provided batchTag's batch.
            // EFGS will return a 404 if those 2 dates do not match.

            // If we do not have a batchTag (it is null or empty), the first batch of the date requested will be returned by EFGS.
            // We may already have that first batch, but we may not have the next batches.

            var result = await _receiverFactory().ExecuteAsync(date, lastWrittenBatchTag);

            while (result != null && downloadCount < _efgsConfig.MaxBatchesPerRun)
            {
                // If we haven't already received the current batchTag, process it
                if (!_iksInDbContext.Received.Any(x => x.BatchTag == result.BatchTag))
                {
                    try
                    {
                        await WriteSingleBatchAsync(result.BatchTag, result.Content);
                        lastWrittenBatchTag = result.BatchTag;
                    }
                    catch (Exception e)
                    {
                        // TODO: check if we want to handle the specific exceptions
                        _logger.WriteEfgsError(e);
                    }
                }

                // Now move on to the next batch, or stop.
                if (!string.IsNullOrEmpty(result.NextBatchTag))
                {
                    result = await _receiverFactory().ExecuteAsync(date, result.NextBatchTag);
                    downloadCount++;
                }
                else
                {
                    // No next batch available, we're done: end the loop.
                    result = null;
                }
            }

            // Update IksInJob
            jobInfo.LastBatchTag = lastWrittenBatchTag;
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
