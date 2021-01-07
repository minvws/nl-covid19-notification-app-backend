
// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

// using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class RemoveExpiredIksOutCommand
    {
        private readonly IksOutDbContext _DbContext;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly RemoveExpiredIksOutCommandLoggingExtensions _Logger;

        public RemoveExpiredIksOutCommand(IksOutDbContext context, IUtcDateTimeProvider dateTimeProvider,
            RemoveExpiredIksOutCommandLoggingExtensions logger)
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

            foreach (var entity in _DbContext.Iks.Where(x => x.Created < cutoffDate))
            {
                _DbContext.Iks.Remove(entity);
                deleted++;
            }

            _DbContext.SaveChanges();

            _Logger.WriteNumberDeleted(deleted);

            _Logger.WriteEnd();
        }
    }
}
