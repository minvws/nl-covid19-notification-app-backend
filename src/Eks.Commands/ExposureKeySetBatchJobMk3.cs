// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    /// <summary>
    /// Snapshots items from Diagnostic Keys table (previously the snapshot targetted the Workflows directly)
    /// Builds an EKS for every 750k DKs
    /// Stuffing is now written back to the DK table. NB this must occur BEFORE the DKS are built or stuffing can be inferred.
    /// NB With at least 2 strategy pattern type changes this is, strictly speaking, the Mk4.
    /// </summary>
    public sealed class ExposureKeySetBatchJobMk3
    {
        private readonly IWrappedEfExtensions _SqlCommands;
        private readonly IEksConfig _EksConfig;
        private readonly IEksBuilder _SetBuilder;
        private readonly IEksStuffingGeneratorMk2 _EksStuffingGenerator;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly EksEngineLoggingExtensions _Logger;
        private readonly ISnapshotEksInput _Snapshotter;
        private readonly MarkDiagnosisKeysAsUsedLocally _MarkWorkFlowTeksAsUsed;

        private readonly IEksJobContentWriter _ContentWriter;
        private readonly IWriteStuffingToDiagnosisKeys _WriteStuffingToDiagnosisKeys;

        private readonly string _JobName;
        private readonly EksEngineResult _EksEngineResult = new EksEngineResult();
        private readonly IList<EksInfo> _EksResults = new List<EksInfo>();

        private readonly List<EksCreateJobInputEntity> _Output;
        private int _EksCount;

        private bool _Fired;
        private readonly Stopwatch _BuildEksStopwatch = new Stopwatch();
        private readonly Func<EksPublishingJobDbContext> _PublishingDbContextFac;

        public ExposureKeySetBatchJobMk3(IEksConfig eksConfig, IEksBuilder builder, Func<EksPublishingJobDbContext> publishingDbContextFac, IUtcDateTimeProvider dateTimeProvider, EksEngineLoggingExtensions logger, IEksStuffingGeneratorMk2 eksStuffingGenerator, ISnapshotEksInput snapshotter, MarkDiagnosisKeysAsUsedLocally markDiagnosisKeysAsUsed, IEksJobContentWriter contentWriter, IWriteStuffingToDiagnosisKeys writeStuffingToDiagnosisKeys, IWrappedEfExtensions sqlCommands)
        {
            _EksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _SetBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _PublishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _EksStuffingGenerator = eksStuffingGenerator ?? throw new ArgumentNullException(nameof(eksStuffingGenerator));
            _Snapshotter = snapshotter;
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _MarkWorkFlowTeksAsUsed = markDiagnosisKeysAsUsed ?? throw new ArgumentNullException(nameof(markDiagnosisKeysAsUsed));
            _ContentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
            _Output = new List<EksCreateJobInputEntity>(_EksConfig.TekCountMax);
            _WriteStuffingToDiagnosisKeys = writeStuffingToDiagnosisKeys ?? throw new ArgumentNullException(nameof(writeStuffingToDiagnosisKeys));
            _JobName = $"ExposureKeySetsJob_{_DateTimeProvider.Snapshot:u}".Replace(" ", "_").Replace(":", "_");
            _SqlCommands = sqlCommands ?? throw new ArgumentNullException(nameof(sqlCommands));
        }

        public async Task<EksEngineResult> ExecuteAsync()
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

            await ClearJobTablesAsync();

            var snapshotResult = await _Snapshotter.ExecuteAsync(_EksEngineResult.Started);
            
            _EksEngineResult.InputCount = snapshotResult.TekInputCount;
            _EksEngineResult.SnapshotSeconds = snapshotResult.SnapshotSeconds;
            _EksEngineResult.TransmissionRiskNoneCount = await GetTransmissionRiskNoneCountAsync();

            if (snapshotResult.TekInputCount != 0)
            {
                await StuffAsync();
                await BuildOutputAsync();
                await CommitResultsAsync();
            }

            _EksEngineResult.TotalSeconds = stopwatch.Elapsed.TotalSeconds;
            _EksEngineResult.EksInfo = _EksResults.ToArray();

            _Logger.WriteReconciliationMatchUsable(_EksEngineResult.ReconcileOutputCount);
            _Logger.WriteReconciliationMatchCount(_EksEngineResult.ReconcileEksSumCount);

            _Logger.WriteFinished(_JobName);

            return _EksEngineResult;
        }

        private async Task<int> GetTransmissionRiskNoneCountAsync()
        {
            await using var dbc = _PublishingDbContextFac();
            return dbc.EksInput.Count(x => x.TransmissionRiskLevel == TransmissionRiskLevel.None);
        }

        private async Task ClearJobTablesAsync()
        {
            _Logger.WriteCleartables();

            await using var dbc = _PublishingDbContextFac();
            await _SqlCommands.TruncateTableAsync(dbc, TableNames.EksEngineInput);
            await _SqlCommands.TruncateTableAsync(dbc, TableNames.EksEngineOutput);
        }

        private async Task StuffAsync()
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


            //TODO Flat distributions by default. If the default changes, delegate to interfaces.
            //TODO Any weighting of these distributions with current data will be done here.
            //TODO If there will never be a weighted version, move this inside the generator.
            var stuffing = _EksStuffingGenerator.Execute(stuffingCount);

            _Logger.WriteStuffingRequired(stuffing.Length);

            await using var tx = dbc.BeginTransaction();
            await dbc.EksInput.AddRangeAsync(stuffing);
            dbc.SaveAndCommit();

            _Logger.WriteStuffingAdded();
        }

        private async Task BuildOutputAsync()
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
                    await WriteNewEksToOutputAsync();
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
                await WriteNewEksToOutputAsync();
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

        private async Task WriteNewEksToOutputAsync()
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
            var result = dbc.EksInput
                .Where(x => x.TransmissionRiskLevel != TransmissionRiskLevel.None)
                .OrderBy(x => x.KeyData)
                .ThenBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToArray();

            _Logger.WriteFinishedReadPage(result.Length);

            return result;
        }

        private async Task CommitResultsAsync()
        {
            _Logger.WriteCommitPublish();

            await _ContentWriter.ExecuteAsync();

            _Logger.WriteCommitMarkTeks();
            var result = await _MarkWorkFlowTeksAsUsed.ExecuteAsync();
            _Logger.WriteTotalMarked(result.Marked);
            

            //Write stuffing to DKs
            await _WriteStuffingToDiagnosisKeys.ExecuteAsync();

            await ClearJobTablesAsync();
        }
   }
}