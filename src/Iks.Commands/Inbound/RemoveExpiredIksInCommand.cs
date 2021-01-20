// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public class RemoveExpiredIksInCommand
    {
        private readonly IksInDbContext _DbContext;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly RemoveExpiredIksInCommandLoggingExtensions _Logger;

        public RemoveExpiredIksInCommand(IksInDbContext context, IUtcDateTimeProvider dateTimeProvider,
            RemoveExpiredIksInCommandLoggingExtensions logger)
        {
            _DbContext = context ?? throw new ArgumentNullException(nameof(context));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Execute()
        {
            var cutoffDate = _DateTimeProvider.Snapshot.AddDays(-14);

            _Logger.WriteStart();

            var deleted = 0;

            foreach (var entity in _DbContext.InJob.Where(x => x.LastRun < cutoffDate))
            {
                _DbContext.InJob.Remove(entity);
                deleted++;
            }

            _DbContext.SaveChanges();

            _Logger.WriteNumberDeleted(deleted);

            _Logger.WriteEnd();
        }
    }
}