// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound
{
    public class IksPollingBatchJob
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly Func<HttpGetIksCommand> _ReceiverFactory;
        private readonly Func<IksWriterCommand> _WriterFactory;
        private readonly IksInDbContext _IksInDbContext;
        private readonly IEfgsConfig _EfgsConfig;

        public IksPollingBatchJob(IUtcDateTimeProvider dateTimeProvider, Func<HttpGetIksCommand> receiverFactory, Func<IksWriterCommand> writerFactory, IksInDbContext iksInDbContext, IEfgsConfig efgsConfig)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ReceiverFactory = receiverFactory ?? throw new ArgumentNullException(nameof(receiverFactory));
            _WriterFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));
            _IksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _EfgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
        }

        public async Task ExecuteAsync()
        {
            var jobInfo = GetJobInfo();

            var batchDate = jobInfo.LastRun;
            var previousBatch = jobInfo.LastBatchTag;
            var batch = jobInfo.LastBatchTag;

            // All of the values on first run are empty, all we need to do is set the date to the first date and we can start
            if (jobInfo.LastRun == DateTime.MinValue)
            {
                batchDate = _DateTimeProvider.Snapshot.Date.AddDays(_EfgsConfig.DaysToDownload * -1);
            }
            
            while (true)
            {
                var result = await _ReceiverFactory().ExecuteAsync(batch, batchDate);

                // Handle errors
                if (result == null)
                {
                    break;
                }

                // Process a previous batch
                if (result.BatchTag == previousBatch)
                {
                    // New batch so process it next time
                    if (result.NextBatchTag != null)
                    {
                        batch = result.NextBatchTag;
                        continue;
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

                    previousBatch = result.BatchTag;

                    // Move to the next batch if we have one
                    if (result.NextBatchTag != null)
                    {
                        batch = result.NextBatchTag;
                        continue;
                    }
                }

                if (batchDate < _DateTimeProvider.Snapshot.Date)
                {
                    batchDate = batchDate.AddDays(1);
                    batch = string.Empty;
                }
                else
                {
                    break;
                }
            }

            // Update the last-run
            jobInfo.LastRun = _DateTimeProvider.Snapshot;
            jobInfo.LastBatchTag = batch;

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