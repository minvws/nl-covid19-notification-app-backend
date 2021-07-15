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
        private readonly IksImportCommand _iksImportCommand;

        public IksImportBatchJob(IUtcDateTimeProvider dateTimeProvider, IksInDbContext iksInDbContext, IksImportCommand iksImportCommand)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _iksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _iksImportCommand = iksImportCommand ?? throw new ArgumentNullException(nameof(iksImportCommand));
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
                await _iksImportCommand.Execute(item);

                if (!item.Error)
                {
                    item.Accepted = _dateTimeProvider.Snapshot;
                }

                _iksInDbContext.Update(item);
                await _iksInDbContext.SaveChangesAsync();
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
