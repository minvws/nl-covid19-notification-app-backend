// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class RemoveLocalDuplicateDiagnosisKeysCommand : IRemoveDuplicateDiagnosisKeysCommand
    {
        private readonly Func<DkSourceDbContext> _dkSourceDbProvider;

        public RemoveLocalDuplicateDiagnosisKeysCommand(Func<DkSourceDbContext> dkSourceDbProvider)
        {
            _dkSourceDbProvider = dkSourceDbProvider ?? throw new ArgumentNullException(nameof(dkSourceDbProvider));

        }

        public async Task ExecuteAsync()
        {
            await using var context = _dkSourceDbProvider.Invoke();
            await using var transaction = context.BeginTransaction();
            context.Database.ExecuteSqlRaw("EXEC dbo.RemoveLocalDuplicateDiagnosisKeys");
            await transaction.CommitAsync();
        }
    }
}
