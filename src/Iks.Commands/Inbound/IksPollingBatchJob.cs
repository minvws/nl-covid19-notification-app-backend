// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var tagToRequest = jobInfo.LastBatchTag;
            var dayToRequest = jobInfo.LastRun == DateTime.MinValue ? _dateTimeProvider.Snapshot.Date.AddDays(_efgsConfig.DaysToDownload * -1) : jobInfo.LastRun;

            var downloadCount = 0;
            var daysLeft = true;

            while (daysLeft && downloadCount <= _efgsConfig.MaxBatchesPerRun)
            {
                var result = await _receiverFactory().ExecuteAsync(dayToRequest, tagToRequest);
                downloadCount++;

                // If we haven't already received the current batchTag, process it
                if (result != null && !_iksInDbContext.Received.AsNoTracking().Any(x => x.BatchTag == result.BatchTag))
                {
                    _logger.WriteProcessingData(dayToRequest, result.BatchTag);

                    try
                    {
                        var iksWriter = _writerFactory();

                        // Persists batch to database
                        await iksWriter.Execute(new IksWriteArgs
                        {
                            BatchTag = result.BatchTag,
                            Content = result.Content
                        });

                        // Update jobinfo
                        jobInfo.LastBatchTag = result.BatchTag;
                        jobInfo.LastRun = dayToRequest;

                        // Persist updated jobinfo to database
                        await _iksInDbContext.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.WriteEfgsError(e);
                    }
                }

                // Move on to the next batchTag if available;
                // otherwise, move on to the next day.
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
