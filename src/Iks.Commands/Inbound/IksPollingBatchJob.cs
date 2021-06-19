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

            // The first batch of the given date will be returned by EFGS; we may already have it.
            var date = jobInfo.LastRun == DateTime.MinValue ? _dateTimeProvider.Snapshot.Date : jobInfo.LastRun;
            var result = await _receiverFactory().ExecuteAsync(date);

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
                    catch (Exception)
                    {
                        //TODO: catch specific exception and handle them properly.
                    }

                }

                // Move on to the next batch, or stop.
                if (!string.IsNullOrEmpty(result.NextBatchTag))
                {
                    // The date is ignored by EFGS when a specific batchTag is requested,
                    // but it is required so let's send it anyway.
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
            var lastBatchTagDate = lastWrittenBatchTag.Split("-").FirstOrDefault();
            var lastRun = !string.IsNullOrEmpty(lastBatchTagDate) ? DateTime.ParseExact(lastBatchTagDate, "yyyyMMdd", null) : _dateTimeProvider.Snapshot.Date;

            jobInfo.LastBatchTag = lastWrittenBatchTag;
            jobInfo.LastRun = lastRun;

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
