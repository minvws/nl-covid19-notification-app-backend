// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class DiagnosiskeyInputEntityDeduplicator
    {
        private readonly DkSourceDbContext _dkSourceDbContext;
        private readonly ILogger _logger;

        public DiagnosiskeyInputEntityDeduplicator(DkSourceDbContext dkSourceDbContext, ILogger<DiagnosiskeyInputEntityDeduplicator> logger)
        {
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<DiagnosisKeyInputEntity>> FilterOutExistingDailyKeys(List<DiagnosisKeyInputEntity> candidates)
        {
            var currentEntriesAsInputEntity = await _dkSourceDbContext.DiagnosisKeys
                .AsNoTracking()
                .Where(p => !p.ReadyForCleanup.HasValue || !p.ReadyForCleanup.Value)
                .Select(x => new DiagnosisKeyInputEntity { DailyKey = x.DailyKey })
                .ToListAsync();

            var result = candidates.Except(currentEntriesAsInputEntity, new DiagnosisKeyInputEntityComparer()).ToList();

            _logger.LogDebug("Filtered {FilteredCount} DKs", candidates.Count - result.Count);

            return result;
        }

        private struct DiagnosisKeyInputEntityComparer : IEqualityComparer<DiagnosisKeyInputEntity>
        {
            public bool Equals(DiagnosisKeyInputEntity x, DiagnosisKeyInputEntity y)
            {
                return y != null
                    && x != null
                    && x.DailyKey.KeyData.SequenceEqual(y.DailyKey.KeyData)
                    && x.DailyKey.RollingPeriod == y.DailyKey.RollingPeriod
                    && x.DailyKey.RollingStartNumber == y.DailyKey.RollingStartNumber;
            }

            public int GetHashCode(DiagnosisKeyInputEntity obj)
            {
                return obj.DailyKey.KeyData.Aggregate(
                    string.Empty,
                    (s, i) => s + i.GetHashCode(), s => s.GetHashCode());
            }
        }
    }
}
