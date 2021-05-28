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
        private readonly SnapshotLoggingExtensions _Logger;
        private readonly DkSourceDbContext _DkSourceDbContext;
        private readonly Func<EksPublishingJobDbContext> _PublishingDbContextFactory;
        private readonly IInfectiousness _infectiousness;

        public SnapshotDiagnosisKeys(SnapshotLoggingExtensions logger, DkSourceDbContext dkSourceDbContext, Func<EksPublishingJobDbContext> publishingDbContextFactory, IInfectiousness infectiousness)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
            _PublishingDbContextFactory = publishingDbContextFactory ?? throw new ArgumentNullException(nameof(publishingDbContextFactory));
            _infectiousness = infectiousness ?? throw new ArgumentNullException(nameof(infectiousness));
        }

        public async Task<SnapshotEksInputResult> ExecuteAsync(DateTime snapshotStart)
        {
            _Logger.WriteStart();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int pageSize = 10000;
            var index = 0;
            var filteredTekInputCount = 0;

            await using var tx = _DkSourceDbContext.BeginTransaction();
            var (filteredResult, pageCount) = ReadAndFilter(index, pageSize);
            
            while (pageCount > 0)
            {
                var db = _PublishingDbContextFactory();
                if (filteredResult.Length > 0)
                {
                    await db.BulkInsertAsync2(filteredResult, new SubsetBulkArgs());
                }

                index += pageCount;
                filteredTekInputCount += filteredResult.Length;
                (filteredResult, pageCount) = ReadAndFilter(index, pageSize);
            }

            var result = new SnapshotEksInputResult
            {
                SnapshotSeconds = stopwatch.Elapsed.TotalSeconds,
                TekInputCount = index,
                FilteredTekInputCount = filteredTekInputCount
            };

            _Logger.WriteTeksToPublish(index);

            return result;
        }

        private (EksCreateJobInputEntity[], int pageCount) ReadAndFilter(int index, int pageSize)
        {
            var result = _DkSourceDbContext.DiagnosisKeys
                .Where(x => !x.PublishedLocally)
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
            var filteredResult = result.Where(x =>
                    _infectiousness.IsInfectious(x.Symptomatic, x.DaysSinceSymptomsOnset))
                .ToArray();

            return (filteredResult, result.Length);
        }
    }
}