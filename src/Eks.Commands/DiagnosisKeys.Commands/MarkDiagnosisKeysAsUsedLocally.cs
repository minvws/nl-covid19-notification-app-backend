// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands
{
    /// <summary>
    /// For interop, replace MarkWorkFlowTeksAsUsed in the EKS Engine Mk3
    /// Also see SnapshotEksInputMk2
    /// Relies on EF Bulk Extensions
    /// </summary>
    public class MarkDiagnosisKeysAsUsedLocally
    {
        private readonly DkSourceDbContext _dkSourceDbContext;
        private readonly EksPublishingJobDbContext _eksPublishingJobDbContext;
        private readonly IEksConfig _eksConfig;
        private readonly ILogger<MarkDiagnosisKeysAsUsedLocally> _logger;
        private int _index;

        public MarkDiagnosisKeysAsUsedLocally(DkSourceDbContext dkDbContext, IEksConfig eksConfig, EksPublishingJobDbContext eksPublishingJobDbContext, ILogger<MarkDiagnosisKeysAsUsedLocally> logger)
        {
            _dkSourceDbContext = dkDbContext ?? throw new ArgumentNullException(nameof(dkDbContext));
            _eksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _eksPublishingJobDbContext = eksPublishingJobDbContext ?? throw new ArgumentNullException(nameof(eksPublishingJobDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MarkDiagnosisKeysAsUsedResult> ExecuteAsync()
        {
            var used = ReadPage();
            while (used.Length > 0)
            {
                await ZapAsync(used);
                used = ReadPage();
            }
            return new MarkDiagnosisKeysAsUsedResult { Marked = _index };
        }

        private async Task ZapAsync(long[] used)
        {
            var zap = _dkSourceDbContext.DiagnosisKeys
                .AsNoTracking()
                .Where(x => used.Contains(x.Id))
                .ToList();

            _index += used.Length;
            _logger.LogInformation("Marking as Published - Count: {Count}, Running total: {RunningTotal}.", zap.Count, _index);

            if (zap.Count == 0)
            {
                return;
            }

            var idsToUpdate = string.Join(",", zap.Select(x => x.Id.ToString()).ToArray());

            await _dkSourceDbContext.BulkUpdateSqlRawAsync(
                tableName: "DiagnosisKeys",
                columnName: "PublishedLocally",
                value: true,
                ids: idsToUpdate);
        }

        private long[] ReadPage()
        {
            //New context each time
            return _eksPublishingJobDbContext.Set<EksCreateJobInputEntity>()
                .AsNoTracking()
                .Where(x => x.TekId != null && (x.Used || x.TransmissionRiskLevel == TransmissionRiskLevel.None))
                .OrderBy(x => x.TekId)
                .Skip(_index)
                .Take(_eksConfig.PageSize)
                .Select(x => x.TekId ?? default)
                .ToArray();
        }
    }
}
