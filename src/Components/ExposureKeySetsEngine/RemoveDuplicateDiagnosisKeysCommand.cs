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
    public class RemoveDuplicateDiagnosisKeysCommand
    {
        private readonly Func<DkSourceDbContext> _DkSourceDbProvider;

        public RemoveDuplicateDiagnosisKeysCommand(Func<DkSourceDbContext> dkSourceDbProvider)
        {
            _DkSourceDbProvider = dkSourceDbProvider ?? throw new ArgumentNullException(nameof(dkSourceDbProvider));
        }

        public async Task ExecuteAsync()
        {
            await using var context = _DkSourceDbProvider.Invoke();
            await using var transaction = context.BeginTransaction();

            var duplicates = context.DiagnosisKeys
                .GroupBy(_ => _.DailyKey)
                .Where(_ => _.Count() > 1)
                .Select(_ => _.Key);

            foreach (var duplicate in duplicates)
            {
                var keys = context.DiagnosisKeys
                    .Where(_ => _.DailyKey.KeyData == duplicate.KeyData
                                && _.DailyKey.RollingPeriod == duplicate.RollingPeriod
                                && _.DailyKey.RollingStartNumber == duplicate.RollingStartNumber)
                    .Select(_ => _)
                    .ToList();

                if (keys.TrueForAll(_ => _.PublishedToEfgs))
                {
                    // Do nothing
                }
                else if (keys.Any(_ => _.PublishedToEfgs))
                {
                    MarkAllAsSent(keys);
                }
                else
                {
                    MarkLowestTransmissionRiskLevelsAsSent(keys);
                }
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private void MarkAllAsSent(IEnumerable<DiagnosisKeyEntity> keys)
        {
            foreach (var key in keys) key.PublishedToEfgs = true;
        }

        private void MarkLowestTransmissionRiskLevelsAsSent(IEnumerable<DiagnosisKeyEntity> keys)
        {
            foreach (var key in keys.OrderByDescending(_ => _.Local.TransmissionRiskLevel).Skip(1))
            {
                key.PublishedToEfgs = true;
            }
        }
    }
}
