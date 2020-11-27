// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound.Publishing
{
    public class IksInputSnapshotCommand
    {
        private readonly ILogger<IksInputSnapshotCommand> _Logger;
        private readonly DkSourceDbContext _DkSourceDbContext;
        private readonly Func<IksPublishingJobDbContext> _PublishingDbContextFactory;
        private readonly IOutboundFixedCountriesOfInterestSetting _Config;

        public IksInputSnapshotCommand(ILogger<IksInputSnapshotCommand> logger, DkSourceDbContext dkSourceDbContext, Func<IksPublishingJobDbContext> tekSourceDbContextFunc, IOutboundFixedCountriesOfInterestSetting config)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
            _PublishingDbContextFactory = tekSourceDbContextFunc ?? throw new ArgumentNullException(nameof(tekSourceDbContextFunc));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<SnapshotIksInputResult> ExecuteAsync()
        {
            _Logger.LogDebug("Snapshot publishable DKs.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int pagesize = 10000;
            var index = 0;

            using var tx = _DkSourceDbContext.BeginTransaction();
            var page = Read(index, pagesize);

            while (page.Count > 0)
            {
                var db = _PublishingDbContextFactory();
                await db.BulkInsertAsync2(page.ToList(), new SubsetBulkArgs());

                index += page.Count;
                page = Read(index, pagesize);
            }

            var result = new SnapshotIksInputResult
            {
                ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                Count = index
            };

            _Logger.LogInformation("DKs to publish - Count:{Count}.", index);

            return result;
        }

        /// <summary>
        /// Maps the Local Tek info to Efgs info using 
        /// </summary>
        private IList<IksCreateJobInputEntity> Read(int index, int pageSize)
        {
            var q1 = _DkSourceDbContext.DiagnosisKeys
            .Where(x => x.Origin == TekOrigin.Local && !x.PublishedToEfgs) //All imported IKS DKs are marked as true at the point of import
            .Skip(index)
            .Take(pageSize)
            .AsNoTracking() //EF treats DTO property classes as 'owned tables'
            .Select(x => new
            {
                DkId = x.Id,
                x.DailyKey,
                Trl = x.Local.TransmissionRiskLevel.Value,
                x.Local.DaysSinceSymptomsOnset,
            }).ToList();

            var q2 = q1.Select(x => new IksCreateJobInputEntity 
            { 
                DkId = x.DkId,
                DaysSinceSymptomsOnset = x.DaysSinceSymptomsOnset.Value,
                TransmissionRiskLevel = x.Trl,
                ReportType = ReportType.ConfirmedClinicalDiagnosis,
                DailyKey = x.DailyKey,
                CountriesOfInterest = string.Join(",", _Config.CountriesOfInterest)
            })
            .ToList();

            return q2;
        }
    }
}