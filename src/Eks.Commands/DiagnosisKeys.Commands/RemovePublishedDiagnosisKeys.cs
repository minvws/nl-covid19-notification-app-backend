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
        private RemovePublishedDiagnosisKeysResult _Result;
        private readonly Func<DkSourceDbContext> _DiagnosticKeyDbContextProvider;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;

        public RemovePublishedDiagnosisKeys(Func<DkSourceDbContext> diagnosticKeyDbContextProvider, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _DiagnosticKeyDbContextProvider = diagnosticKeyDbContextProvider ?? throw new ArgumentNullException(nameof(diagnosticKeyDbContextProvider));
            _UtcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
        }

        public RemovePublishedDiagnosisKeysResult Execute()
        {
            if (_Result != null)
                throw new InvalidOperationException("Object already used.");

            _Result = new RemovePublishedDiagnosisKeysResult();

            //TODO setting
            var cutoff = _UtcDateTimeProvider.Snapshot.AddDays(-14).Date.ToRollingStartNumber();

            using (var dbc = _DiagnosticKeyDbContextProvider())
            {
                using (var tx = dbc.BeginTransaction())
                {
                    _Result.GivenMercy = dbc.Database.ExecuteSqlRaw($"DELETE FROM {TableNames.DiagnosisKeys} WHERE [PublishedLocally] = 1 AND [PublishedToEfgs] = 1 AND DailyKey_RollingStartNumber < {cutoff};");
                    tx.Commit();
                }

                _Result.RemainingExpiredCount = dbc.DiagnosisKeys.Count(x => x.DailyKey.RollingStartNumber < cutoff);
            }

            return _Result;
        }
    }
}
