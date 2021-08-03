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
    public class RemoveDiagnosisKeysReadyForCleanupCommand : BaseCommand
    {
        private readonly DkSourceDbContext _diagnosticKeyDbContext;

        public RemoveDiagnosisKeysReadyForCleanupCommand(DkSourceDbContext diagnosticKeyDbContext)
        {
            _diagnosticKeyDbContext = diagnosticKeyDbContext ?? throw new ArgumentNullException(nameof(diagnosticKeyDbContext));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            await _diagnosticKeyDbContext.BulkDeleteWithTransactionAsync(_diagnosticKeyDbContext.DiagnosisKeys.AsNoTracking().Where(p => p.ReadyForCleanup.Value).ToList(), new SubsetBulkArgs());
            return null;
        }
    }
}
