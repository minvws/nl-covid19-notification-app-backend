// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands
{
    public class RemoveDiagnosisKeysReadyForCleanup
    {
        private readonly DkSourceDbContext _diagnosticKeyDbContext;

        public RemoveDiagnosisKeysReadyForCleanup(DkSourceDbContext diagnosticKeyDbContext)
        {
            _diagnosticKeyDbContext = diagnosticKeyDbContext ?? throw new ArgumentNullException(nameof(diagnosticKeyDbContext));
        }

        public async Task ExecuteAsync()
        {
            await _diagnosticKeyDbContext.BulkDeleteAsync(_diagnosticKeyDbContext.DiagnosisKeys.AsNoTracking().Where(p => p.ReadyForCleanup.Value).ToList());
        }
    }
}
