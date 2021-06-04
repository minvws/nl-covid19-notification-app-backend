// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing
{
    public class IksImportBatchJob
    {

        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IksInDbContext _iksInDbContext;
        private readonly Func<IksImportCommand> _inboundIksReaderFunc;

        public IksImportBatchJob(IUtcDateTimeProvider dateTimeProvider, IksInDbContext iksInDbContext, Func<IksImportCommand> inboundIksReaderFunc)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _iksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _inboundIksReaderFunc = inboundIksReaderFunc ?? throw new ArgumentNullException(nameof(inboundIksReaderFunc));
        }

        public async Task ExecuteAsync()
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
                var processor = _inboundIksReaderFunc();
                await processor.Execute(item);

                if (!item.Error)
                    item.Accepted = _dateTimeProvider.Snapshot;

                _iksInDbContext.Update(item);
                _iksInDbContext.SaveChanges();
            }
            catch (Exception e)
            {
                //TODO mark as having issues - retry x times? Don't want to repeat
                //TODO log...
                throw;
            }
        }

        private IksInEntity GetItem()
        {
            return _iksInDbContext.Received
                .Where(x => x.Content != null && x.Accepted == null && !x.Error)
                .OrderBy(x => x.Created)
                .Take(1)
                .SingleOrDefault();
        }
    }
}
