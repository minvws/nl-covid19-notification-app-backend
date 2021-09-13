// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        private readonly DkSourceDbContext _dkSourceDbContext;
        private readonly IksPublishingJobDbContext _iksPublishingJobDbContext;
        private readonly IIksConfig _iksConfig;
        private readonly ILogger<MarkDiagnosisKeysAsUsedByIks> _logger;
        private int _index;

        public MarkDiagnosisKeysAsUsedByIks(DkSourceDbContext dkSourceDbContext, IIksConfig config, IksPublishingJobDbContext iksPublishingJobDbContext, ILogger<MarkDiagnosisKeysAsUsedByIks> logger)
        {
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
            _iksConfig = config ?? throw new ArgumentNullException(nameof(config));
            _iksPublishingJobDbContext = iksPublishingJobDbContext ?? throw new ArgumentNullException(nameof(iksPublishingJobDbContext));
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
            var diagnosisKeyEntities = _dkSourceDbContext.DiagnosisKeys
                .AsNoTracking()
                .Where(x => used.Contains(x.Id))
                .ToList();

            _index += used.Length;
            _logger.LogInformation("Marking as Published - Count:{Count}, Running total:{RunningTotal}.", diagnosisKeyEntities.Count, _index);

            if (diagnosisKeyEntities.Count == 0)
            {
                return;
            }

            foreach (var i in diagnosisKeyEntities)
            {
                i.PublishedToEfgs = true;
            }

            var bulkArgs = new SubsetBulkArgs
            {
                PropertiesToInclude = new[] { $"{nameof(DiagnosisKeyEntity.PublishedToEfgs)}" }
            };

            await _dkSourceDbContext.BulkUpdateWithTransactionAsync(diagnosisKeyEntities, bulkArgs);
        }

        private long[] ReadPage()
        {
            //New context each time
            return _iksPublishingJobDbContext.Input
                .AsNoTracking()
                .Where(x => x.Used)
                .OrderBy(x => x.DkId)
                .Skip(_index)
                .Take(10000)
                .Select(x => x.DkId)
                .ToArray();
        }
    }
}
