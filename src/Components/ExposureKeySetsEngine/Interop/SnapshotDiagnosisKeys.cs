// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop
{
    /// <summary>
    /// Snapshot EKS input from DKS table
    /// TODO extra filters?
    /// </summary>
    public class SnapshotDiagnosisKeys : ISnapshotEksInput
    {
        private readonly ILogger<SnapshotDiagnosisKeys> _Logger;
        private readonly DkSourceDbContext _DkSourceDbContext;
        private readonly Func<EksPublishingJobDbContext> _PublishingDbContextFactory;

        public SnapshotDiagnosisKeys(ILogger<SnapshotDiagnosisKeys> logger, DkSourceDbContext dkSourceDbContext, Func<EksPublishingJobDbContext> publishingDbContextFactory)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
            _PublishingDbContextFactory = publishingDbContextFactory ?? throw new ArgumentNullException(nameof(publishingDbContextFactory));
        }

        public async Task<SnapshotEksInputResult> ExecuteAsync(DateTime snapshotStart)
        {
            _Logger.LogDebug("Snapshot publishable DKs.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int pagesize = 10000;
            var index = 0;

            using var tx = _DkSourceDbContext.BeginTransaction();
            var page = Read(index, pagesize);
            
            while (page.Length > 0)
            {
                var db = _PublishingDbContextFactory();
                await db.BulkInsertAsync2(page, new SubsetBulkArgs());

                index += page.Length;
                page = Read(index, pagesize);
            }

            var result = new SnapshotEksInputResult
            {
                SnapshotSeconds = stopwatch.Elapsed.TotalSeconds,
                TekInputCount = index
            };

            _Logger.LogInformation("TEKs to publish - Count:{Count}.", index);

            return result;
        }

        private EksCreateJobInputEntity[] Read(int index, int pageSize)
            => _DkSourceDbContext.DiagnosisKeys
                .Where(x => !x.PublishedLocally)
                .OrderBy(x => x.Id)
                .AsNoTracking()
                .Skip(index)
                .Take(pageSize)
                .Select(x => new EksCreateJobInputEntity {
                    TekId = x.Id,
                    KeyData = x.DailyKey.KeyData,
                    RollingStartNumber = x.DailyKey.RollingStartNumber,
                    RollingPeriod = x.DailyKey.RollingPeriod, 
                    TransmissionRiskLevel = x.Local.TransmissionRiskLevel.Value
                }).ToArray();
    }
}