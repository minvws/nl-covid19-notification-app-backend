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
        private readonly Func<DkSourceDbContext> _dkDbContextFactory;
        private readonly IIksConfig _iksConfig;
        private readonly Func<IksPublishingJobDbContext> _publishingDbContextFac;
        private readonly ILogger<MarkDiagnosisKeysAsUsedByIks> _logger;
        private int _index;

        public MarkDiagnosisKeysAsUsedByIks(Func<DkSourceDbContext> dkDbContextFactory, IIksConfig config, Func<IksPublishingJobDbContext> publishingDbContextFac, ILogger<MarkDiagnosisKeysAsUsedByIks> logger)
        {
            _dkDbContextFactory = dkDbContextFactory ?? throw new ArgumentNullException(nameof(dkDbContextFactory));
            _iksConfig = config ?? throw new ArgumentNullException(nameof(config));
            _publishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MarkDksAsUsedResult> ExecuteAsync()
        {
            var used = ReadPage();
            while (used.Length > 0)
            {
                await Zap(used);
                used = ReadPage();
            }
            return new MarkDksAsUsedResult { Count = _index };
        }

        private async Task Zap(long[] used)
        {
            await using var wfDb = _dkDbContextFactory();

            var zap = wfDb.DiagnosisKeys
                .Where(x => used.Contains(x.Id))
                .ToList();

            _index += used.Length;
            _logger.LogInformation("Marking as Published - Count:{Count}, Running total:{RunningTotal}.", zap.Count, _index);

            if (zap.Count == 0)
            {
                return;
            }

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
            return _publishingDbContextFac().Input
                .Where(x => x.Used)
                .OrderBy(x => x.DkId)
                .Skip(_index)
                .Take(_iksConfig.PageSize)
                .Select(x => x.DkId)
                .ToArray();
        }
    }
}
