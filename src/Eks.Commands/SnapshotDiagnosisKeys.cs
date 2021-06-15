// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    /// <summary>
    /// Snapshot EKS input from DKS table
    /// TODO extra filters?
    /// </summary>
    public class SnapshotDiagnosisKeys : ISnapshotEksInput
    {
        private readonly SnapshotLoggingExtensions _logger;
        private readonly DkSourceDbContext _dkSourceDbContext;
        private readonly Func<EksPublishingJobDbContext> _publishingDbContextFactory;
        private readonly IInfectiousness _infectiousness;

        public SnapshotDiagnosisKeys(SnapshotLoggingExtensions logger, DkSourceDbContext dkSourceDbContext, Func<EksPublishingJobDbContext> publishingDbContextFactory, IInfectiousness infectiousness)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
            _publishingDbContextFactory = publishingDbContextFactory ?? throw new ArgumentNullException(nameof(publishingDbContextFactory));
            _infectiousness = infectiousness ?? throw new ArgumentNullException(nameof(infectiousness));
        }

        public async Task<SnapshotEksInputResult> ExecuteAsync(DateTime snapshotStart)
        {
            _logger.WriteStart();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int PageSize = 10000;
            var index = 0;
            var filteredTekInputCount = 0;

            await using var tx = _dkSourceDbContext.BeginTransaction();
            var (page, filteredResult) = ReadAndFilter(index, PageSize);

            while (page.Length > 0)
            {
                MarkFilteredEntitiesForCleanup(page, filteredResult);

                var db = _publishingDbContextFactory();
                if (filteredResult.Length > 0)
                {
                    await db.BulkInsertAsync2(filteredResult, new SubsetBulkArgs());
                }

                index += page.Length;
                filteredTekInputCount += filteredResult.Length;
                (page, filteredResult) = ReadAndFilter(index, PageSize);
            }

            _dkSourceDbContext.SaveAndCommit();

            var snapshotEksInputResult = new SnapshotEksInputResult
            {
                SnapshotSeconds = stopwatch.Elapsed.TotalSeconds,
                TekInputCount = index,
                FilteredTekInputCount = filteredTekInputCount
            };

            _logger.WriteTeksToPublish(index);

            return snapshotEksInputResult;
        }

        private void MarkFilteredEntitiesForCleanup(EksCreateJobInputEntity[] allEntities, EksCreateJobInputEntity[] filteredResult)
        {
            var leftoverDKIds = allEntities.Except(filteredResult).Select(x => x.TekId).ToArray();
            if(leftoverDKIds.Any())
            {
                var dksToMarkForCleanup = _dkSourceDbContext.DiagnosisKeys.Where(x => leftoverDKIds.Contains(x.Id)).ToList();
                foreach (var diagnosisKeyEntity in dksToMarkForCleanup)
                {
                    diagnosisKeyEntity.ReadyForCleanup = true;
                }
            }
        }

        private (EksCreateJobInputEntity[], EksCreateJobInputEntity[]) ReadAndFilter(int index, int pageSize)
        {
            var page = _dkSourceDbContext.DiagnosisKeys
                .Where(x => !x.PublishedLocally && (!x.ReadyForCleanup.HasValue || !x.ReadyForCleanup.Value))
                .OrderBy(x => x.Id)
                .AsNoTracking()
                .Skip(index)
                .Take(pageSize)
                .Select(x => new EksCreateJobInputEntity
                {
                    TekId = x.Id,
                    KeyData = x.DailyKey.KeyData,
                    RollingStartNumber = x.DailyKey.RollingStartNumber,
                    RollingPeriod = x.DailyKey.RollingPeriod,
                    TransmissionRiskLevel = x.Local.TransmissionRiskLevel.Value,
                    DaysSinceSymptomsOnset = x.Local.DaysSinceSymptomsOnset.Value,
                    Symptomatic = x.Local.Symptomatic,
                    ReportType = x.Local.ReportType
                }).ToArray();

            // Filter the List of EksCreateJobInputEntities by the RiskCalculationParameter filters
            var filteredResult = page.Where(x =>
                    _infectiousness.IsInfectious(x.Symptomatic, x.DaysSinceSymptomsOnset))
                .ToArray();

            return (page, filteredResult);
        }
    }
}
