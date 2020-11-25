// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound
{
    public class IksImportBatchJob
    {

        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IksInDbContext _IksInDbContext;
        private Func<IksImportCommand> _InboundIksReaderFunc;

        public IksImportBatchJob(IUtcDateTimeProvider dateTimeProvider, IksInDbContext iksInDbContext, Func<IksImportCommand> inboundIksReaderFunc)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _IksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _InboundIksReaderFunc = inboundIksReaderFunc ?? throw new ArgumentNullException(nameof(inboundIksReaderFunc));
        }

        public async Task Execute()
        {
            var item = GetItem();
            while (item != null)
            {
                await Process(item);
                item = GetItem();
            }
        }

        private async Task Process(IksInEntity item)
        {
            try
            {
                var processor = _InboundIksReaderFunc();
                await processor.Execute(item);
                item.Accepted = _DateTimeProvider.Snapshot;
                _IksInDbContext.Update(item);
                await _IksInDbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                //TODO mark as having issues - retry x times? Don't want to repeat
                //TODO log...
                throw;
            }
        }

        private IksInEntity? GetItem()
        {
            return _IksInDbContext.Received
                .Where(x => x.Content != null && x.Accepted == null)
                .OrderBy(x => x.Created)
                .Take(1)
                .SingleOrDefault();
        }
    }
}