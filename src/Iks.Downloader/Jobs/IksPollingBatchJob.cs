// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net;
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
        private readonly IHttpGetIksCommand _batchDownloader;
        private readonly IIksWriterCommand _writer;
        private readonly IksInDbContext _iksInDbContext;
        private readonly IEfgsConfig _efgsConfig;
        private readonly IksDownloaderLoggingExtensions _logger;

        private IksInJobEntity _jobInfo { get; set; }
        private string _tagToRequest { get; set; }
        private DateTime _dayToRequest { get; set; }
        private int _downloadCount { get; set; }
        private bool _continueDownloading { get; set; }
        private HttpGetIksResult _downloadedBatch { get; set; }

        public IksPollingBatchJob(
            IUtcDateTimeProvider dateTimeProvider,
            IHttpGetIksCommand receiver,
            IIksWriterCommand writer,
            IksInDbContext iksInDbContext,
            IEfgsConfig efgsConfig,
            IksDownloaderLoggingExtensions logger)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _batchDownloader = receiver ?? throw new ArgumentNullException(nameof(receiver));
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

            _downloadCount = 0;
            _continueDownloading = true;
            _tagToRequest = string.Empty;
            _dayToRequest = _dateTimeProvider.Snapshot.Date;

            SetInitialBatchTagAndDate();

            while (_continueDownloading && _downloadCount <= _efgsConfig.MaxBatchesPerRun)
            {
                _downloadedBatch = _batchDownloader.ExecuteAsync(_dayToRequest, _tagToRequest).GetAwaiter().GetResult();
                _downloadCount++;

                if (_downloadedBatch.ResultCode == HttpStatusCode.OK)
                {
                    ProcessBatch();
                    SetNextBatchtagAndDate();
                }
                else
                {
                    ProcessErrors();
                }
            }
        }

        private void SetInitialBatchTagAndDate()
        {
            var lastJob = _iksInDbContext.InJob.SingleOrDefault();

            if (lastJob == null)
            {
                lastJob = new IksInJobEntity();
                _iksInDbContext.InJob.Add(lastJob);
            }
            _jobInfo = lastJob;

            var lastWrittenBatchTag = _jobInfo.LastBatchTag;
            var dateFromBatchtag = lastWrittenBatchTag.Split("-").FirstOrDefault();

            if (string.IsNullOrEmpty(dateFromBatchtag))
            {
                // LastWrittenBatchTag is somehow unusable or unavailable
                // Set requestDate as far back as allowed and keep requestBatchTag empty
                _dayToRequest = _dayToRequest.AddDays(-1 * _efgsConfig.DaysToDownload);
            }
            else
            {
                // LastWrittenBatchTag is useable: continue where we left off.
                // The date and the batchTag's creation date need to be the same, otherwise EFGS will return a 404.
                _dayToRequest = DateTime.ParseExact(dateFromBatchtag, "yyyyMMdd", null);
                _tagToRequest = lastWrittenBatchTag;
            }
        }

        private void ProcessBatch()
        {
            var batchAlreadyReceived = _iksInDbContext.Received.AsNoTracking().Any(x => x.BatchTag == _downloadedBatch.BatchTag);

            if (batchAlreadyReceived)
            {
                _logger.WriteBatchAlreadyProcessed(_downloadedBatch.BatchTag);
            }
            else
            {
                _logger.WriteProcessingData(_dayToRequest, _downloadedBatch.BatchTag);

                try
                {
                    WriteBatchToDb();
                    WriteRunToDb();
                }
                catch (Exception e)
                {
                    _logger.WriteEfgsError(e);
                }
            }
        }

        private void ProcessErrors()
        {
            switch (_downloadedBatch.ResultCode)
            {
                case HttpStatusCode.BadRequest:
                    // 400: requested batchtag doesn't match CreatedDate on found batch - halt and catch fire
                    throw new EfgsCommunicationException($"Request with date '{_downloadedBatch.RequestedDay}' and batchTag '{_downloadedBatch.BatchTag}' resulted in a Bad Request-response");

                case HttpStatusCode.NotFound:
                    // 404: requested date doesn't exist yet - retry later (stop downloading)
                    _logger.WriteResponseNotFound();
                    WriteRunToDb();
                    _continueDownloading = false;
                    return;

                case HttpStatusCode.Gone:
                    // 410: requested date is too old - skip to next day
                    _logger.WriteResponseGone();
                    WriteRunToDb();
                    _tagToRequest = string.Empty;
                    IncrementDate();

                    return;

                default:
                    throw new ArgumentException();
            }
        }

        private void SetNextBatchtagAndDate()
        {
            // Move on to the next batchTag if available; otherwise, move on to the next day, if possible.
            _tagToRequest = _downloadedBatch?.NextBatchTag ?? string.Empty;

            if (string.IsNullOrEmpty(_tagToRequest))
            {
                _logger.WriteNoNextBatch();
                IncrementDate();
            }
            else
            {
                // Log for informational purposes
                _logger.WriteNextBatchFound(_tagToRequest);
                _logger.WriteBatchProcessedInNextLoop(_tagToRequest);
            }

            // Log for informational purposes
            if (_downloadCount > _efgsConfig.MaxBatchesPerRun)
            {
                _logger.WriteBatchMaximumReached(_efgsConfig.MaxBatchesPerRun);
            }
        }

        private void IncrementDate()
        {
            var batchWasDownloadedFromPreviousDay = _downloadedBatch?.RequestedDay.Date < _dateTimeProvider.Snapshot.Date
                    || _dayToRequest < _dateTimeProvider.Snapshot.Date;

            if (batchWasDownloadedFromPreviousDay)
            {
                _logger.WriteMovingToNextDay();
                _dayToRequest = _dayToRequest.AddDays(1);
            }
            else
            {
                _logger.WriteNoNextBatchNoMoreDays();
                _continueDownloading = false;
            }
        }

        private void WriteBatchToDb()
        {
            _writer.Execute(new IksWriteArgs
            {
                BatchTag = _downloadedBatch.BatchTag,
                Content = _downloadedBatch.Content
            });
        }

        private void WriteRunToDb()
        {
            _jobInfo.LastRun = _downloadedBatch.RequestedDay;
            _jobInfo.LastBatchTag = _downloadedBatch.BatchTag;

            _iksInDbContext.SaveChanges();
        }

    }
}
