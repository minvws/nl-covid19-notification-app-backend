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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    /// <summary>
    /// Add database IO to the job
    /// </summary>
    public sealed class ExposureKeySetBatchJobMk3
    {
        private readonly IEksConfig _EksConfig;
        private readonly IEksBuilder _SetBuilder;
        private readonly IEksStuffingGenerator _EksStuffingGenerator;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly EksEngineLoggingExtensions _Logger;
        private readonly ISnapshotEksInput _Snapshotter;
        private readonly IMarkWorkFlowTeksAsUsed _MarkWorkFlowTeksAsUsed;

        private readonly EksJobContentWriter _ContentWriter;

        private readonly string _JobName;
        private readonly EksEngineResult _EksEngineResult = new EksEngineResult();
        private readonly IList<EksInfo> _EksResults = new List<EksInfo>();

        private readonly List<EksCreateJobInputEntity> _Output;
        private int _EksCount;

        private bool _Fired;
        private readonly Stopwatch _BuildEksStopwatch = new Stopwatch();
        private readonly Func<PublishingJobDbContext> _PublishingDbContextFac;

        public ExposureKeySetBatchJobMk3(IEksConfig eksConfig, IEksBuilder builder, Func<PublishingJobDbContext> publishingDbContextFac, IUtcDateTimeProvider dateTimeProvider, EksEngineLoggingExtensions logger, IEksStuffingGenerator eksStuffingGenerator, ISnapshotEksInput snapshotter, IMarkWorkFlowTeksAsUsed markWorkFlowTeksAsUsed, EksJobContentWriter contentWriter)
        {
            //_JobConfig = jobConfig;
            _EksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _SetBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _PublishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _EksStuffingGenerator = eksStuffingGenerator ?? throw new ArgumentNullException(nameof(eksStuffingGenerator));
            _Snapshotter = snapshotter;
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _MarkWorkFlowTeksAsUsed = markWorkFlowTeksAsUsed ?? throw new ArgumentNullException(nameof(markWorkFlowTeksAsUsed));
            _ContentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
            _Output = new List<EksCreateJobInputEntity>(_EksConfig.TekCountMax);
            _JobName = $"ExposureKeySetsJob_{_DateTimeProvider.Snapshot:u}".Replace(" ", "_").Replace(":", "_");
        }

        public async Task<EksEngineResult> Execute()
        {
            if (_Fired)
                throw new InvalidOperationException("One use only.");

            _Fired = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _Logger.WriteStart(_JobName);

            if (Environment.UserInteractive && !WindowsIdentityQueries.CurrentUserIsAdministrator())
                _Logger.WriteNoElevatedPrivs(_JobName);

            _EksEngineResult.Started = _DateTimeProvider.Snapshot; //Align with the logged job name.

            await ClearJobTables();

            var snapshotResult = await _Snapshotter.Execute(_EksEngineResult.Started);
            
            _EksEngineResult.InputCount = snapshotResult.TekInputCount;
            _EksEngineResult.SnapshotSeconds = snapshotResult.SnapshotSeconds;
            _EksEngineResult.TransmissionRiskNoneCount = await GetTransmissionRiskNoneCount();

            if (snapshotResult.TekInputCount != 0)
            {
                await Stuff();
                await BuildOutput();
                await CommitResults();
            }

            _EksEngineResult.TotalSeconds = stopwatch.Elapsed.TotalSeconds;
            _EksEngineResult.EksInfo = _EksResults.ToArray();

            _Logger.WriteReconciliationMatchUsable(_EksEngineResult.ReconcileOutputCount);
            _Logger.WriteReconciliationMatchCount(_EksEngineResult.ReconcileEksSumCount);

            _Logger.WriteFinished(_JobName);

            return _EksEngineResult;
        }

        private async Task<int> GetTransmissionRiskNoneCount()
        {
            await using var dbc = _PublishingDbContextFac();
            return dbc.EksInput.Count(x => x.TransmissionRiskLevel == TransmissionRiskLevel.None);
        }

        private async Task ClearJobTables()
        {
            _Logger.WriteCleartables();

            await using var dbc = _PublishingDbContextFac();
            await using var tx = dbc.BeginTransaction();
            await dbc.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE [dbo].[{TableNames.EksEngineInput}]");
            await dbc.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE [dbo].[{TableNames.EksEngineOutput}]");
            tx.Commit();
        }

        private async Task Stuff()
        {
            await using var dbc = _PublishingDbContextFac();
            var tekCount = dbc.EksInput.Count(x => x.TransmissionRiskLevel != TransmissionRiskLevel.None);

            if (tekCount == 0)
            {
                _Logger.WriteNoStuffingNoTeks();
                return;
            }

            var stuffingCount = tekCount < _EksConfig.TekCountMin ? _EksConfig.TekCountMin - tekCount : 0;
            if (stuffingCount == 0)
            {
                _Logger.WriteNoStuffingMinimumOk();
                return;
            }

            _EksEngineResult.StuffingCount = stuffingCount;

            var stuffing = _EksStuffingGenerator.Execute(new StuffingArgs {Count = stuffingCount, JobTime = _EksEngineResult.Started});

            _Logger.WriteStuffingRequired(stuffing.Length);

            await using var tx = dbc.BeginTransaction();
            await dbc.EksInput.AddRangeAsync(stuffing);
            dbc.SaveAndCommit();

            _Logger.WriteStuffingAdded();
        }




        private async Task BuildOutput()
        {
            _Logger.WriteBuildEkses();
            _BuildEksStopwatch.Start();

            var inputIndex = 0;
            var page = GetInputPage(inputIndex, _EksConfig.PageSize);
            _Logger.WriteReadTeks(page.Length);

            while (page.Length > 0)
            {
                if (_Output.Count + page.Length >= _EksConfig.TekCountMax)
                {
                    _Logger.WritePageFillsToCapacity(_EksConfig.TekCountMax);
                    var remainingCapacity = _EksConfig.TekCountMax - _Output.Count;
                    AddToOutput(page.Take(remainingCapacity).ToArray()); //Fill to max
                    await WriteNewEksToOutput();
                    AddToOutput(page.Skip(remainingCapacity).ToArray()); //Use leftovers from the page
                }
                else
                {
                    AddToOutput(page);
                }

                inputIndex += _EksConfig.PageSize; //Move input index
                page = GetInputPage(inputIndex, _EksConfig.PageSize); //Read next page.
                _Logger.WriteReadTeks(page.Length);
            }

            if (_Output.Count > 0)
            {
                _Logger.WriteRemainingTeks(_Output.Count);
                await WriteNewEksToOutput();
            }
        }

        private void AddToOutput(EksCreateJobInputEntity[] page)
        {
            _Output.AddRange(page); //Lots of memory
            _Logger.WriteAddTeksToOutput(page.Length, _Output.Count);
        }

        private static TemporaryExposureKeyArgs Map(EksCreateJobInputEntity c)
            => new TemporaryExposureKeyArgs 
            { 
                RollingPeriod = c.RollingPeriod,
                TransmissionRiskLevel = c.TransmissionRiskLevel,
                KeyData = c.KeyData,
                RollingStartNumber = c.RollingStartNumber
            };

        private async Task WriteNewEksToOutput()
        {
            _Logger.WriteBuildEntry();

            var args = _Output.Select(Map).ToArray();

            var content = await _SetBuilder.BuildAsync(args);
            
            var e = new EksCreateJobOutputEntity
            {
                Region = DefaultValues.Region,
                Release = _EksEngineResult.Started,
                CreatingJobQualifier = ++_EksCount,
                Content = content, 
            };

            _Logger.WriteWritingCurrentEks(e.CreatingJobQualifier);

            
            await using (var dbc = _PublishingDbContextFac())
            {
                await using var tx = dbc.BeginTransaction();
                await dbc.AddAsync(e);
                dbc.SaveAndCommit();
            }

            _Logger.WriteMarkTekAsUsed();

            foreach (var i in _Output)
                i.Used = true;

            //Could be 750k in this hit
            await using (var dbc2 = _PublishingDbContextFac())
            {
                var bargs = new SubsetBulkArgs
                {
                    PropertiesToInclude = new[] {nameof(EksCreateJobInputEntity.Used)}
                };
                await dbc2.BulkUpdateAsync2(_Output, bargs); //TX
            }

            _EksEngineResult.OutputCount += _Output.Count;

            _EksResults.Add(new EksInfo { TekCount = _Output.Count, TotalSeconds = _BuildEksStopwatch.Elapsed.TotalSeconds });
            _Output.Clear();
        }

        private EksCreateJobInputEntity[] GetInputPage(int skip, int take)
        {
            _Logger.WriteStartReadPage(skip, take);

            using var dbc = _PublishingDbContextFac();
            var result = dbc.Set<EksCreateJobInputEntity>()
                .Where(x => x.TransmissionRiskLevel != TransmissionRiskLevel.None)
                .OrderBy(x => x.KeyData).Skip(skip).Take(take).ToArray();

            _Logger.WriteFinishedReadPage(result.Length);

            return result;
        }

        private async Task CommitResults()
        {
            _Logger.WriteCommitPublish();

            await _ContentWriter.ExecuteAsyc();

            _Logger.WriteCommitMarkTeks();
            var result = await _MarkWorkFlowTeksAsUsed.ExecuteAsync();
            _Logger.WriteTotalMarked(result.Marked);
            
            await ClearJobTables();
        }
   }
}