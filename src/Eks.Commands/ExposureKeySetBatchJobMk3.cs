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
        private readonly IWrappedEfExtensions _sqlCommands;
        private readonly IEksConfig _eksConfig;
        private readonly IEksBuilder _setBuilder;
        private readonly IEksStuffingGeneratorMk2 _eksStuffingGenerator;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly EksEngineLoggingExtensions _logger;
        private readonly ISnapshotEksInput _snapshotter;
        private readonly MarkDiagnosisKeysAsUsedLocally _markWorkFlowTeksAsUsed;

        private readonly IEksJobContentWriter _contentWriter;
        private readonly IWriteStuffingToDiagnosisKeys _writeStuffingToDiagnosisKeys;

        private readonly string _jobName;
        private readonly EksEngineResult _eksEngineResult = new EksEngineResult();
        private readonly IList<EksInfo> _eksResults = new List<EksInfo>();

        private readonly List<EksCreateJobInputEntity> _output;
        private int _eksCount;

        private bool _fired;
        private readonly Stopwatch _buildEksStopwatch = new Stopwatch();
        private readonly Func<EksPublishingJobDbContext> _publishingDbContextFac;

        public ExposureKeySetBatchJobMk3(IEksConfig eksConfig, IEksBuilder builder, Func<EksPublishingJobDbContext> publishingDbContextFac, IUtcDateTimeProvider dateTimeProvider, EksEngineLoggingExtensions logger, IEksStuffingGeneratorMk2 eksStuffingGenerator, ISnapshotEksInput snapshotter, MarkDiagnosisKeysAsUsedLocally markDiagnosisKeysAsUsed, IEksJobContentWriter contentWriter, IWriteStuffingToDiagnosisKeys writeStuffingToDiagnosisKeys, IWrappedEfExtensions sqlCommands)
        {
            _eksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _setBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _publishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _eksStuffingGenerator = eksStuffingGenerator ?? throw new ArgumentNullException(nameof(eksStuffingGenerator));
            _snapshotter = snapshotter;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _markWorkFlowTeksAsUsed = markDiagnosisKeysAsUsed ?? throw new ArgumentNullException(nameof(markDiagnosisKeysAsUsed));
            _contentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
            _output = new List<EksCreateJobInputEntity>(_eksConfig.TekCountMax);
            _writeStuffingToDiagnosisKeys = writeStuffingToDiagnosisKeys ?? throw new ArgumentNullException(nameof(writeStuffingToDiagnosisKeys));
            _jobName = $"ExposureKeySetsJob_{_dateTimeProvider.Snapshot:u}".Replace(" ", "_").Replace(":", "_");
            _sqlCommands = sqlCommands ?? throw new ArgumentNullException(nameof(sqlCommands));
        }

        public async Task<EksEngineResult> ExecuteAsync()
        {
            if (_fired)
                throw new InvalidOperationException("One use only.");

            _fired = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _logger.WriteStart(_jobName);

            if (Environment.UserInteractive && !WindowsIdentityQueries.CurrentUserIsAdministrator())
            {
                _logger.WriteNoElevatedPrivs(_jobName);
            }

            _eksEngineResult.Started = _dateTimeProvider.Snapshot; //Align with the logged job name.

            await ClearJobTablesAsync();

            var snapshotResult = await _snapshotter.ExecuteAsync(_eksEngineResult.Started);
            
            _eksEngineResult.InputCount = snapshotResult.TekInputCount;
            _eksEngineResult.FilteredInputCount = snapshotResult.FilteredTekInputCount;
            _eksEngineResult.SnapshotSeconds = snapshotResult.SnapshotSeconds;
            _eksEngineResult.TransmissionRiskNoneCount = await GetTransmissionRiskNoneCountAsync();

            if (snapshotResult.TekInputCount != 0)
            {
                await StuffAsync();
                await BuildOutputAsync();
                await CommitResultsAsync();
            }

            _eksEngineResult.TotalSeconds = stopwatch.Elapsed.TotalSeconds;
            _eksEngineResult.EksInfo = _eksResults.ToArray();

            _logger.WriteReconciliationMatchUsable(_eksEngineResult.ReconcileOutputCount);
            _logger.WriteReconciliationMatchCount(_eksEngineResult.ReconcileEksSumCount);

            _logger.WriteFinished(_jobName);

            return _eksEngineResult;
        }

        private async Task<int> GetTransmissionRiskNoneCountAsync()
        {
            await using var dbc = _publishingDbContextFac();
            return dbc.EksInput.Count(x => x.TransmissionRiskLevel == TransmissionRiskLevel.None);
        }

        private async Task ClearJobTablesAsync()
        {
            _logger.WriteCleartables();

            var dbc = _publishingDbContextFac();
            await _sqlCommands.TruncateTableAsync(dbc, TableNames.EksEngineInput);
            await _sqlCommands.TruncateTableAsync(dbc, TableNames.EksEngineOutput);
        }

        private async Task StuffAsync()
        {
            if (_eksEngineResult.InputCount == 0)
            {
                _logger.WriteNoStuffingNoTeks();
                return;
            }
            
            await using var dbc = _publishingDbContextFac();
            var tekCount = dbc.EksInput.Count(x => x.TransmissionRiskLevel != TransmissionRiskLevel.None);
            
            var stuffingCount = tekCount < _eksConfig.TekCountMin ? _eksConfig.TekCountMin - tekCount : 0;
            if (stuffingCount == 0)
            {
                _logger.WriteNoStuffingMinimumOk();
                return;
            }

            _eksEngineResult.StuffingCount = stuffingCount;


            //TODO Flat distributions by default. If the default changes, delegate to interfaces.
            //TODO Any weighting of these distributions with current data will be done here.
            //TODO If there will never be a weighted version, move this inside the generator.
            var stuffing = _eksStuffingGenerator.Execute(stuffingCount);

            _logger.WriteStuffingRequired(stuffing.Length);

            await using var tx = dbc.BeginTransaction();
            await dbc.EksInput.AddRangeAsync(stuffing);
            dbc.SaveAndCommit();

            _logger.WriteStuffingAdded();
        }

        private async Task BuildOutputAsync()
        {
            _logger.WriteBuildEkses();
            _buildEksStopwatch.Start();

            var inputIndex = 0;
            var page = GetInputPage(inputIndex, _eksConfig.PageSize);
            _logger.WriteReadTeks(page.Length);

            while (page.Length > 0)
            {
                if (_output.Count + page.Length >= _eksConfig.TekCountMax)
                {
                    _logger.WritePageFillsToCapacity(_eksConfig.TekCountMax);
                    var remainingCapacity = _eksConfig.TekCountMax - _output.Count;
                    AddToOutput(page.Take(remainingCapacity).ToArray()); //Fill to max
                    await WriteNewEksToOutputAsync();
                    AddToOutput(page.Skip(remainingCapacity).ToArray()); //Use leftovers from the page
                }
                else
                {
                    AddToOutput(page);
                }

                inputIndex += _eksConfig.PageSize; //Move input index
                page = GetInputPage(inputIndex, _eksConfig.PageSize); //Read next page.
                _logger.WriteReadTeks(page.Length);
            }

            if (_output.Count > 0)
            {
                _logger.WriteRemainingTeks(_output.Count);
                await WriteNewEksToOutputAsync();
            }
        }

        private void AddToOutput(EksCreateJobInputEntity[] page)
        {
            _output.AddRange(page); //Lots of memory
            _logger.WriteAddTeksToOutput(page.Length, _output.Count);
        }

        private static TemporaryExposureKeyArgs Map(EksCreateJobInputEntity c)
            => new TemporaryExposureKeyArgs 
            { 
                RollingPeriod = c.RollingPeriod,
                TransmissionRiskLevel = c.TransmissionRiskLevel,
                KeyData = c.KeyData,
                RollingStartNumber = c.RollingStartNumber,
                DaysSinceOnsetSymptoms = c.DaysSinceSymptomsOnset,
                Symptomatic = c.Symptomatic,
                ReportType = c.ReportType
            };

        private async Task WriteNewEksToOutputAsync()
        {
            _logger.WriteBuildEntry();

            var args = _output.Select(Map).ToArray();

            var content = await _setBuilder.BuildAsync(args);
            
            var e = new EksCreateJobOutputEntity
            {
                Region = DefaultValues.Region,
                Release = _eksEngineResult.Started,
                CreatingJobQualifier = ++_eksCount,
                Content = content, 
            };

            _logger.WriteWritingCurrentEks(e.CreatingJobQualifier);

            
            await using (var dbc = _publishingDbContextFac())
            {
                await using var tx = dbc.BeginTransaction();
                await dbc.AddAsync(e);
                dbc.SaveAndCommit();
            }

            _logger.WriteMarkTekAsUsed();

            foreach (var i in _output)
                i.Used = true;

            //Could be 750k in this hit
            await using (var dbc2 = _publishingDbContextFac())
            {
                var bargs = new SubsetBulkArgs
                {
                    PropertiesToInclude = new[] {nameof(EksCreateJobInputEntity.Used)}
                };
                await dbc2.BulkUpdateAsync2(_output, bargs); //TX
            }

            _eksEngineResult.OutputCount += _output.Count;

            _eksResults.Add(new EksInfo { TekCount = _output.Count, TotalSeconds = _buildEksStopwatch.Elapsed.TotalSeconds });
            _output.Clear();
        }

        private EksCreateJobInputEntity[] GetInputPage(int skip, int take)
        {
            _logger.WriteStartReadPage(skip, take);

            using var dbc = _publishingDbContextFac();
            var unFilteredResult = dbc.EksInput
                .OrderBy(x => x.KeyData)
                .ThenBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToArray();

            _logger.WriteFinishedReadPage(unFilteredResult.Length);

            return unFilteredResult;
        }

        private async Task CommitResultsAsync()
        {
            _logger.WriteCommitPublish();

            await _contentWriter.ExecuteAsync();

            _logger.WriteCommitMarkTeks();
            var result = await _markWorkFlowTeksAsUsed.ExecuteAsync();
            _logger.WriteTotalMarked(result.Marked);
            

            //Write stuffing to DKs
            await _writeStuffingToDiagnosisKeys.ExecuteAsync();

            await ClearJobTablesAsync();
        }
   }
}