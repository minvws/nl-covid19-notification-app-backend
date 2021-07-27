// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands
{
    public class RemovePublishedDiagnosisKeysCommand : BaseCommand
    {
        private RemovePublishedDiagnosisKeysResult _result;
        private readonly DkSourceDbContext _diagnosticKeyDbContext;
        private readonly IUtcDateTimeProvider _utcDateTimeProvider;

        public RemovePublishedDiagnosisKeysCommand(DkSourceDbContext diagnosticKeyDbContext, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _diagnosticKeyDbContext = diagnosticKeyDbContext ?? throw new ArgumentNullException(nameof(diagnosticKeyDbContext));
            _utcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _result = new RemovePublishedDiagnosisKeysResult();

            //TODO setting
            var cutoff = _utcDateTimeProvider.Snapshot.AddDays(-14).Date.ToRollingStartNumber();

            var resultToDelete = await _diagnosticKeyDbContext.DiagnosisKeys.AsNoTracking().Where(p =>
                p.PublishedLocally && p.PublishedToEfgs && p.DailyKey.RollingStartNumber < cutoff).ToArrayAsync();

            _result.GivenMercy = resultToDelete.Length;
            await _diagnosticKeyDbContext.BulkDeleteAsync(resultToDelete);

            _result.RemainingExpiredCount = _diagnosticKeyDbContext.DiagnosisKeys.Count(x => x.DailyKey.RollingStartNumber < cutoff);

            return _result;
        }
    }
}
