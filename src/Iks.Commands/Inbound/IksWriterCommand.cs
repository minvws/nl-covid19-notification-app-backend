// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public class IksWriterCommand : IIksWriterCommand
    {
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IksInDbContext _dbContext;

        public IksWriterCommand(IUtcDateTimeProvider dateTimeProvider, IksInDbContext dbContext)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IksWriteArgs args)
        {
            var e = new IksInEntity
            {
                BatchTag = args.BatchTag,
                Content = args.Content,
                Created = _dateTimeProvider.Snapshot
                //Received = ? Not sure we need this cos its just a log.
            };

            await _dbContext.Received.AddAsync(e);
            await _dbContext.SaveChangesAsync();
        }
    }
}
