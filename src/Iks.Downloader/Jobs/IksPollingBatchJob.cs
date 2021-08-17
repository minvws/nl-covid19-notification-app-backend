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
                }

                SetNextBatchtagAndDate();
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
                }
                catch (Exception e)
                {
                    _logger.WriteEfgsError(e);
                }
            }
        }

        private void SetNextBatchtagAndDate()
        {
            // Move on to the next batchTag if available; otherwise, move on to the next day.
            // In case of erroneous results: either move to next batchTag or next day.

            if (_downloadedBatch.ResultCode == HttpStatusCode.BadRequest || _downloadedBatch.ResultCode == HttpStatusCode.NotFound)
            {
                // In case of 400 or 404: skip to next day
                var batchWasDownloadedFromPreviousDay = _downloadedBatch?.RequestedDay.Date < _dateTimeProvider.Snapshot.Date
                    || _dayToRequest < _dateTimeProvider.Snapshot.Date;

                if (batchWasDownloadedFromPreviousDay)
                {
                    _logger.WriteMovingToNextDay();
                    _dayToRequest = _dayToRequest.AddDays(1);
                    _tagToRequest = string.Empty;
                }
                else
                {
                    _logger.WriteNoNextBatchNoMoreDays();
                    _continueDownloading = false;
                }
                return;

                // - This 400 response is returned by EFGS when for some reason a batchTag is requested that has a different "created date" on their end, then the date we send along in the request.
                // - This scenario shouldn't happen, but when it does, it is useful to not stop downloading keys altogether, but rather to move on to the next day and request the first key of *that* day.
                // - When requesting date from EFGS, if you do not provide a specific batchTag, but just a date, it will return the first batchTag for that date. This seems the right way to recover
                //   from a situation wherein somehow a requested batchTag/date combination leads to a 400 response.

                //var nextDayDate = date.AddDays(1); // Request for next day
                //var batchTag = $"{nextDayDate:yyyyMMdd}";
                //var uri = new Uri($"{_efgsConfig.BaseUrl}/diagnosiskeys/download/{nextDayDate:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);
                //var request = BuildHttpRequest(batchTag, date);

                //_logger.WriteRequest(request);

                //response = await _httpClient.SendAsync(request);

                //_logger.WriteResponse(response.StatusCode);
                //_logger.WriteResponseHeaders(response.Headers);

                //// Handle response
                //return await HandleResponse(date, response);
            }

            if (_downloadedBatch.ResultCode == HttpStatusCode.Gone)
            {
                // In case of 410: try to download next batch with increased batch-number
                var batchTagData = _downloadedBatch.BatchTag.Split("-").ToList();
                var batchTagNumber = Convert.ToInt32(batchTagData.LastOrDefault());
                batchTagData[batchTagData.Count - 1] = (batchTagNumber + 1).ToString();

                _tagToRequest = string.Join('-', batchTagData);
                return;
            }

            _tagToRequest = _downloadedBatch?.NextBatchTag ?? string.Empty;

            if (string.IsNullOrEmpty(_tagToRequest))
            {
                _logger.WriteNoNextBatch();

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

        private void WriteBatchToDb()
        {
            _writer.Execute(new IksWriteArgs
            {
                BatchTag = _downloadedBatch.BatchTag,
                Content = _downloadedBatch.Content
            });

            // Keep track of the last batch we wrote to the database
            // And keep track of the last time this job was run in a functionally meaningful way
            _jobInfo.LastBatchTag = _downloadedBatch.BatchTag;
            _jobInfo.LastRun = _downloadedBatch.RequestedDay;

            _iksInDbContext.SaveChanges();
        }
    }
}
