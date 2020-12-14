// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class RemoveDuplicateDiagnosisKeysForIksCommand
    {
        private readonly Func<DkSourceDbContext> _DkSourceDbProvider;

        public RemoveDuplicateDiagnosisKeysForIksCommand(Func<DkSourceDbContext> dkSourceDbProvider)
        {
            _DkSourceDbProvider = dkSourceDbProvider ?? throw new ArgumentNullException(nameof(dkSourceDbProvider));
        }

        public async Task ExecuteAsync()
        {
            await using var context = _DkSourceDbProvider.Invoke();
            await using var transaction = context.BeginTransaction();

            var duplicates = context.DiagnosisKeys
                .GroupBy(x => x.DailyKey)
                .Where(x => x.Count() > 1)
                .Where(duplicate => !duplicate.All(x => x.PublishedToEfgs));

            foreach (var duplicate in duplicates)
            {
                if (duplicate.Any(x => x.PublishedToEfgs))
                    MarkAllAsSent(duplicate);
                else
                    MarkLowestTransmissionRiskLevelsAsSent(duplicate);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private static void MarkAllAsSent(IEnumerable<DiagnosisKeyEntity> keys)
        {
            foreach (var key in keys) key.PublishedToEfgs = true;
        }

        private static void MarkLowestTransmissionRiskLevelsAsSent(IEnumerable<DiagnosisKeyEntity> keys)
        {
            foreach (var key in keys.OrderByDescending(x => x.Local.TransmissionRiskLevel).Skip(1))
                key.PublishedToEfgs = true;
        }
    }
}
