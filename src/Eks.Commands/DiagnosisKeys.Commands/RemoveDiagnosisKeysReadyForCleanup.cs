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
    public class RemoveDiagnosisKeysReadyForCleanup
    {
        private readonly Func<DkSourceDbContext> _diagnosticKeyDbContextProvider;


        public RemoveDiagnosisKeysReadyForCleanup(Func<DkSourceDbContext> diagnosticKeyDbContextProvider)
        {
            _diagnosticKeyDbContextProvider = diagnosticKeyDbContextProvider ?? throw new ArgumentNullException(nameof(diagnosticKeyDbContextProvider));
        }

        public async Task ExecuteAsync()
        {
            await using var dbc = _diagnosticKeyDbContextProvider();
            await using var tx = dbc.BeginTransaction();

            await dbc.Database.ExecuteSqlRawAsync($"DELETE FROM {TableNames.DiagnosisKeys} WHERE [ReadyForCleanup] = 1;");
            await tx.CommitAsync();
        }
    }
}
