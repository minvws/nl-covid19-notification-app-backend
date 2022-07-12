// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.DiagnosisKeys
{
    public class RemoveDiagnosisKeysReadyForCleanupCommand : BaseCommand
    {
        private readonly DiagnosisKeysDbContext _diagnosticKeyDbContext;

        public RemoveDiagnosisKeysReadyForCleanupCommand(DiagnosisKeysDbContext diagnosticKeyDbContext)
        {
            _diagnosticKeyDbContext = diagnosticKeyDbContext ?? throw new ArgumentNullException(nameof(diagnosticKeyDbContext));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            await _diagnosticKeyDbContext.BulkDeleteSqlRawAsync<DiagnosisKeyEntity>(
                columnName: "ready_for_cleanup",
                checkValue: true);

            return null;
        }
    }
}
