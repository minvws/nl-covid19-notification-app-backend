// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
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

        public RemovePublishedDiagnosisKeysResult Execute()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _result = new RemovePublishedDiagnosisKeysResult();

            //TODO setting
            var cutoff = _utcDateTimeProvider.Snapshot.AddDays(-14).Date.ToRollingStartNumber();

            using (var tx = _diagnosticKeyDbContext.BeginTransaction())
            {
                _result.GivenMercy = _diagnosticKeyDbContext.Database.ExecuteSqlRaw($"DELETE FROM {TableNames.DiagnosisKeys} WHERE [PublishedLocally] = 1 AND [PublishedToEfgs] = 1 AND DailyKey_RollingStartNumber < {cutoff};");
                tx.Commit();
            }

            _result.RemainingExpiredCount = _diagnosticKeyDbContext.DiagnosisKeys.Count(x => x.DailyKey.RollingStartNumber < cutoff);

            return _result;
        }
    }
}
