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

        private IksInJobEntity _jobInfo;
        private string _tagToRequest;
        private DateTime _dayToRequest;
        private int _downloadCount;
        private bool _continueDownloading;

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
                var downloadedBatch = _batchDownloader.ExecuteAsync(_dayToRequest, _tagToRequest).GetAwaiter().GetResult();
                _downloadCount++;

                if (downloadedBatch.ResultCode == HttpStatusCode.OK)
                {
                    ProcessBatch(downloadedBatch);
                    SetNextBatchTagAndDate(downloadedBatch.NextBatchTag);
                }
                else
                {
                    ProcessErrors(downloadedBatch);
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
            var dateFromBatchTag = lastWrittenBatchTag.Split("-").FirstOrDefault();

            if (string.IsNullOrEmpty(dateFromBatchTag))
            {
                // LastWrittenBatchTag is somehow unusable or unavailable
                // Set requestDate as far back as allowed and keep requestBatchTag empty
                _dayToRequest = _dayToRequest.AddDays(-1 * _efgsConfig.DaysToDownload);
            }
            else
            {
                // LastWrittenBatchTag is useable: continue where we left off.
                // The date and the batchTag's creation date need to be the same, otherwise EFGS will return a 404.
                _dayToRequest = DateTime.ParseExact(dateFromBatchTag, "yyyyMMdd", null);
                _tagToRequest = lastWrittenBatchTag;
            }
        }

        private void ProcessBatch(HttpGetIksResult downloadedBatch)
        {
            var batchAlreadyReceived = _iksInDbContext.Received.AsNoTracking().Any(x => x.BatchTag == downloadedBatch.BatchTag);

            if (batchAlreadyReceived)
            {
                _logger.WriteBatchAlreadyProcessed(downloadedBatch.BatchTag);
            }
            else
            {
                _logger.WriteProcessingData(_dayToRequest, downloadedBatch.BatchTag);

                try
                {
                    WriteBatchToDb(downloadedBatch);
                    WriteRunToDb(_dayToRequest, downloadedBatch.BatchTag);
                }
                catch (Exception e)
                {
                    _logger.WriteEfgsError(e);
                }
            }
        }

        private void ProcessErrors(HttpGetIksResult downloadedBatch)
        {
            switch (downloadedBatch.ResultCode)
            {
                case HttpStatusCode.BadRequest:
                    // 400: requested batchtag doesn't match CreatedDate on found batch - halt and catch fire
                    throw new EfgsCommunicationException($"Request with date '{_dayToRequest.Date}' and batchTag '{downloadedBatch.BatchTag}' resulted in a Bad Request-response");

                case HttpStatusCode.NotFound:
                    // 404: requested date doesn't exist yet - retry later (stop downloading)
                    _logger.WriteResponseNotFound();
                    WriteRunToDb(_dayToRequest, downloadedBatch.BatchTag);
                    _continueDownloading = false;
                    return;

                case HttpStatusCode.Gone:
                    // 410: requested date is too old - skip to next day
                    _logger.WriteResponseGone();
                    WriteRunToDb(_dayToRequest, downloadedBatch.BatchTag);
                    _tagToRequest = string.Empty;
                    IncrementDayToRequest();
                    return;

                default:
                    throw new ArgumentException();
            }
        }

        private void SetNextBatchTagAndDate(string? downloadedBatchTag)
        {
            // Move on to the next batchTag if available; otherwise, move on to the next day, if possible.
            _tagToRequest = downloadedBatchTag ?? string.Empty;

            if (string.IsNullOrEmpty(_tagToRequest))
            {
                _logger.WriteNoNextBatch();
                IncrementDayToRequest();
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

        private void IncrementDayToRequest()
        {
            if (_dayToRequest < _dateTimeProvider.Snapshot.Date)
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

        private void WriteBatchToDb(HttpGetIksResult downloadedBatch)
        {
            _writer.Execute(new IksWriteArgs
            {
                BatchTag = downloadedBatch.BatchTag,
                Content = downloadedBatch.Content
            });
        }

        private void WriteRunToDb(DateTime requestedDate, string lastDownloadedBatchTag)
        {
            _jobInfo.LastRun = requestedDate.Date;
            _jobInfo.LastBatchTag = lastDownloadedBatchTag;

            _iksInDbContext.SaveChanges();
        }

    }
}
