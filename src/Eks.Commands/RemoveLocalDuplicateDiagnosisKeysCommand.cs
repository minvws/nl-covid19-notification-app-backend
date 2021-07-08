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
        private readonly DkSourceDbContext _dkSourceDbContext;

        public RemoveLocalDuplicateDiagnosisKeysCommand(DkSourceDbContext dkSourceDbContext)
        {
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
        }

        public async Task ExecuteAsync()
        {
            await using var transaction = _dkSourceDbContext.BeginTransaction();
            await _dkSourceDbContext.Database.ExecuteSqlRawAsync("EXEC dbo.RemoveLocalDuplicateDiagnosisKeys");
            await transaction.CommitAsync();
        }
    }
}
