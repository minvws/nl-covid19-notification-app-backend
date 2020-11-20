// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop
{
    /// <summary>
    /// For interop, replace MarkWorkFlowTeksAsUsed in the EKS Engine Mk3
    /// Also see SnapshotEksInputMk2
    /// Relies on EF Bulk Extensions
    /// </summary>
    public class MarkDiagnosisKeysAsUsed : IMarkDiagnosisKeysAsUsed
    {
        private readonly Func<DkSourceDbContext> _DkDbContextFactory;
        private readonly IEksConfig _EksConfig;
        private readonly Func<EksPublishingJobDbContext> _PublishingDbContextFac;
        private readonly ILogger<MarkDiagnosisKeysAsUsed> _Logger;
        private int _Index;

        public MarkDiagnosisKeysAsUsed(Func<DkSourceDbContext> dkDbContextFactory, IEksConfig eksConfig, Func<EksPublishingJobDbContext> publishingDbContextFac, ILogger<MarkDiagnosisKeysAsUsed> logger)
        {
            _DkDbContextFactory = dkDbContextFactory ?? throw new ArgumentNullException(nameof(dkDbContextFactory));
            _EksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _PublishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MarkDiagnosisKeysAsUsedResult> ExecuteAsync()
        {
            var used = ReadPage();
            while (used.Length > 0)
            {
                await ZapAsync(used);
                used = ReadPage();
            }
            return new MarkDiagnosisKeysAsUsedResult { Marked = _Index };
        }

        private async Task ZapAsync(long[] used)
        {
            await using var wfDb = _DkDbContextFactory();

            var zap = wfDb.DiagnosisKeys
                .Where(x => used.Contains(x.Id))
                .ToList();

            _Index += used.Length;
            _Logger.LogInformation("Marking as Published - Count:{Count}, Running total:{RunningTotal}.", zap.Count, _Index);

            if (zap.Count == 0)
                return;

            foreach (var i in zap)
            {
                i.PublishedLocally = true;
            }

            var bargs = new SubsetBulkArgs 
            {
                PropertiesToInclude = new [] { $"{nameof(DiagnosisKeyEntity.PublishedLocally)}" }
            };
                
            await wfDb.BulkUpdateAsync2(zap, bargs); //TX
        }

        private long[] ReadPage()
        {
            //No tx cos nothing else is touching this context.
            //New context each time
            return _PublishingDbContextFac().Set<EksCreateJobInputEntity>()
                .Where(x => x.TekId != null && (x.Used || x.TransmissionRiskLevel == TransmissionRiskLevel.None))
                .OrderBy(x => x.TekId)
                .Skip(_Index)
                .Take(_EksConfig.PageSize)
                .Select(x => x.TekId.Value)
                .ToArray();
        }
    }
}