// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop
{
    public class RemovePublishedDiagnosticKeys
    {
        private RemovePublishedDiagnosticKeysResult _Result;
        private readonly Func<DkSourceDbContext> _DiagnosticKeyDbContextProvider;

        public RemovePublishedDiagnosticKeys(Func<DkSourceDbContext> diagnosticKeyDbContextProvider)
        {
            _DiagnosticKeyDbContextProvider = diagnosticKeyDbContextProvider ?? throw new ArgumentNullException(nameof(diagnosticKeyDbContextProvider));
        }

        public RemovePublishedDiagnosticKeysResult Execute()
        {
            if (_Result != null)
                throw new InvalidOperationException("Object already used.");

            _Result = new RemovePublishedDiagnosticKeysResult();

            using (var dbc = _DiagnosticKeyDbContextProvider())
            {
                using (var tx = dbc.BeginTransaction())
                {
                    _Result.GivenMercy = dbc.Database.ExecuteSqlRaw($"DELETE FROM {TableNames.DiagnosisKeys} WHERE [PublishedLocally] = 1 AND [PublishedToEfgs] = 1;");
                    tx.Commit();
                }
            }

            return _Result;
        }
    }
}
