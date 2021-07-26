// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing
{
    public class IksImportBatchJob : IJob
    {
        private readonly ILogger<IksImportBatchJob> _logger;
        private readonly IEksEngineConfig _eksEngineConfig;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IksInDbContext _iksInDbContext;
        private readonly CommandInvoker _commandInvoker;
        private readonly IksImportCommand _iksImportCommand;

        public IksImportBatchJob(ILogger<IksImportBatchJob> logger, IEksEngineConfig eksEngineConfig, IUtcDateTimeProvider dateTimeProvider, IksInDbContext iksInDbContext, CommandInvoker commandInvoker, IksImportCommand iksImportCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eksEngineConfig = eksEngineConfig ?? throw new ArgumentNullException(nameof(eksEngineConfig));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _iksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
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
                _commandInvoker.SetCommand(_iksImportCommand, new IksImportCommand.Parameters { IksInEntity = item });
                await _commandInvoker.ExecuteAsync();

                if (!item.Error)
                {
                    item.Accepted = _dateTimeProvider.Snapshot;
                }

                _iksInDbContext.Update(item);
                await _iksInDbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
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

        public void Run()
        {
            if (_eksEngineConfig.IksImportEnabled)
            {
                ExecuteAsync().GetAwaiter().GetResult();
            }
            else
            {
                _logger.LogInformation("IksImport is disabled; Iks files will not be processed.");
            }
        }
    }
}
