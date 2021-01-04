// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing
{
    public class MarkDiagnosisKeysAsUsedByIks
    {
        private readonly Func<DkSourceDbContext> _DkDbContextFactory;
        private readonly IIksConfig _IksConfig;
        private readonly Func<IksPublishingJobDbContext> _PublishingDbContextFac;
        private readonly ILogger<MarkDiagnosisKeysAsUsedByIks> _Logger;
        private int _Index;

        public MarkDiagnosisKeysAsUsedByIks(Func<DkSourceDbContext> dkDbContextFactory, IIksConfig config, Func<IksPublishingJobDbContext> publishingDbContextFac, ILogger<MarkDiagnosisKeysAsUsedByIks> logger)
        {
            _DkDbContextFactory = dkDbContextFactory ?? throw new ArgumentNullException(nameof(dkDbContextFactory));
            _IksConfig = config ?? throw new ArgumentNullException(nameof(config));
            _PublishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MarkDksAsUsedResult> ExecuteAsync()
        {
            var used = ReadPage();
            while (used.Length > 0)
            {
                await Zap(used);
                used = ReadPage();
            }
            return new MarkDksAsUsedResult { Count = _Index };
        }

        private async Task Zap(long[] used)
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
                i.PublishedToEfgs = true;
            }

            var bargs = new SubsetBulkArgs
            {
                PropertiesToInclude = new[] { $"{nameof(DiagnosisKeyEntity.PublishedToEfgs)}" }
            };

            await wfDb.BulkUpdateAsync2(zap, bargs); //TX
        }

        private long[] ReadPage()
        {
            //No tx cos nothing else is touching this context.
            //New context each time
            return _PublishingDbContextFac().Input
                .Where(x => x.Used)
                .OrderBy(x => x.DkId)
                .Skip(_Index)
                .Take(_IksConfig.PageSize)
                .Select(x => x.DkId)
                .ToArray();
        }
    }
}