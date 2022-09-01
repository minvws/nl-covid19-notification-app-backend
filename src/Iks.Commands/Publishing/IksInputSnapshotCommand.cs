// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing
{
    public class IksInputSnapshotCommand
    {
        private readonly ILogger<IksInputSnapshotCommand> _logger;
        private readonly DiagnosisKeysDbContext _diagnosisKeysDbContext;
        private readonly IksPublishingJobDbContext _iksPublishingJobDbContext;
        private readonly IOutboundFixedCountriesOfInterestSetting _config;

        public IksInputSnapshotCommand(ILogger<IksInputSnapshotCommand> logger, DiagnosisKeysDbContext diagnosisKeysDbContext, IksPublishingJobDbContext iksPublishingJobDbContext, IOutboundFixedCountriesOfInterestSetting config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _diagnosisKeysDbContext = diagnosisKeysDbContext ?? throw new ArgumentNullException(nameof(diagnosisKeysDbContext));
            _iksPublishingJobDbContext = iksPublishingJobDbContext ?? throw new ArgumentNullException(nameof(iksPublishingJobDbContext));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public SnapshotIksInputResult Execute()
        {
            _logger.LogDebug("Snapshot publishable DKs.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int PageSize = 10000;
            var index = 0;

            var page = Read(index, PageSize);

            while (page.Count > 0)
            {
                _iksPublishingJobDbContext.BulkInsertBinaryCopy(page);

                index += page.Count;
                page = Read(index, PageSize);
            }

            var result = new SnapshotIksInputResult
            {
                ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                Count = index
            };

            _logger.LogInformation("DKs to publish - Count: {Count}.", index);

            return result;
        }

        /// <summary>
        /// Maps the Local Tek info to Efgs info using 
        /// </summary>
        private IList<IksCreateJobInputEntity> Read(int index, int pageSize)
        {
            //All imported IKS DKs are mark PublishedToEfgs as true at the point of import
            var q1 = _diagnosisKeysDbContext.DiagnosisKeys
                .AsNoTracking() //EF treats DTO property classes as 'owned tables'
                .Where(x => x.Origin == TekOrigin.Local && !x.PublishedToEfgs)
                .Skip(index)
                .Take(pageSize)
                .Select(x => new { x.Efgs.DaysSinceSymptomsOnset, x.DailyKey, Dkid = x.Id });

            var q1A = q1.Where(x => x.DaysSinceSymptomsOnset.HasValue)
            .Select(x => new
            {
                x.Dkid,
                x.DailyKey,
                x.DaysSinceSymptomsOnset,
            });

            var q2 = q1A.Select(x => new IksCreateJobInputEntity
            {
                DkId = x.Dkid,
                DaysSinceSymptomsOnset = x.DaysSinceSymptomsOnset ?? default,
                TransmissionRiskLevel = TransmissionRiskLevel.None, //Remove; this isn't in used in any calculations
                ReportType = ReportType.ConfirmedTest,
                DailyKey = x.DailyKey,
                CountriesOfInterest = string.Join(",", _config.CountriesOfInterest)
            })
            .ToList();

            return q2;
        }
    }
}
