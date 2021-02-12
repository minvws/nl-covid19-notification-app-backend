// // Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// // Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// // SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class RemoveDuplicateDiagnosisKeysForIksWithSpCommand : IRemoveDuplicateDiagnosisKeysForIksCommand
    {
        private readonly Func<DkSourceDbContext> _DkSourceDbProvider;

        public RemoveDuplicateDiagnosisKeysForIksWithSpCommand(Func<DkSourceDbContext> dkSourceDbProvider)
        {
            _DkSourceDbProvider = dkSourceDbProvider ?? throw new ArgumentNullException(nameof(dkSourceDbProvider));
            
        }

        public async Task ExecuteAsync()
        {
            await using var context = _DkSourceDbProvider.Invoke();
            await using var transaction = context.BeginTransaction();
            context.Database.ExecuteSqlRaw("EXEC dbo.RemoveDuplicateDiagnosisKeysForIks");
            await transaction.CommitAsync();
        }
    }
}