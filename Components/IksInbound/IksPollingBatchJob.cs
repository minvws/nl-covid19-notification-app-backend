// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound
{
    public class IksPollingBatchJob
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        private readonly Func<HttpGetIksCommand> _ReceiverFactory;
        private readonly Func<IksWriterCommand> _WriterFactory;
        private readonly IksInDbContext _IksInDbContext;
        private readonly List<HttpGetIksResult> _Results = new List<HttpGetIksResult>();

        public IksPollingBatchJob(IUtcDateTimeProvider dateTimeProvider, Func<HttpGetIksCommand> receiverFactory, Func<IksWriterCommand> writerFactory, IksInDbContext iksInDbContext)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ReceiverFactory = receiverFactory ?? throw new ArgumentNullException(nameof(receiverFactory));
            _WriterFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));
            _IksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
        }

        public async Task<IksPollingResult> ExecuteAsync()
        {
            var jobInfo = GetJobInfo();
            var getResult = await _ReceiverFactory().ExecuteAsync(jobInfo.LastBatchTag);

            while (getResult.HttpStatusCode == HttpStatusCode.OK && getResult.SuccessInfo != null)
            {
                _Results.Add(getResult);

                var writer = _WriterFactory();

                await writer.Execute(new IksWriteArgs
                {
                    BatchTag = getResult.SuccessInfo.BatchTag,
                    Content = getResult.SuccessInfo.Content
                });

                if (string.IsNullOrWhiteSpace(getResult.SuccessInfo.NextBatchTag))
                    break;

                getResult = await _ReceiverFactory().ExecuteAsync(getResult.SuccessInfo.NextBatchTag);
            }

            jobInfo.LastRun = _DateTimeProvider.Snapshot;
            jobInfo.LastBatchTag = _Results.LastOrDefault()?.SuccessInfo.BatchTag ?? string.Empty;
            await _IksInDbContext.SaveChangesAsync();

            return new IksPollingResult
            {
                ErrorItem = (getResult.HttpStatusCode == HttpStatusCode.OK && getResult.Exception) ? null : getResult,
                Items = _Results.Select(x => x.SuccessInfo).ToArray()
            };
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