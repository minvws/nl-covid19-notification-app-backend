// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands
{
    public class RemovePublishedDiagnosisKeys
    {
        private RemovePublishedDiagnosisKeysResult _result;
        private readonly DkSourceDbContext _diagnosticKeyDbContext;
        private readonly IUtcDateTimeProvider _utcDateTimeProvider;

        public RemovePublishedDiagnosisKeys(DkSourceDbContext diagnosticKeyDbContext, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _diagnosticKeyDbContext = diagnosticKeyDbContext ?? throw new ArgumentNullException(nameof(diagnosticKeyDbContext));
            _utcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
        }

        public async Task<RemovePublishedDiagnosisKeysResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _result = new RemovePublishedDiagnosisKeysResult();

            //TODO setting
            var cutoff = _utcDateTimeProvider.Snapshot.AddDays(-14).Date.ToRollingStartNumber();

            var resultToDelete = _diagnosticKeyDbContext.DiagnosisKeys.AsNoTracking().Where(p =>
                p.PublishedLocally && p.PublishedToEfgs && p.DailyKey.RollingStartNumber < cutoff).ToList();

            _result.GivenMercy = resultToDelete.Count;
            await _diagnosticKeyDbContext.BulkDeleteWithTransactionAsync(resultToDelete, new SubsetBulkArgs());

            _result.RemainingExpiredCount = _diagnosticKeyDbContext.DiagnosisKeys.Count(x => x.DailyKey.RollingStartNumber < cutoff);

            return _result;
        }
    }
}
