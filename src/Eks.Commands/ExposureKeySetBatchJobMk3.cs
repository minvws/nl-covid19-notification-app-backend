// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public sealed class ExposureKeySetBatchJobMk3 : BaseCommand
    {
        private readonly IEksConfig _eksConfig;
        private readonly IEksBuilder _setBuilder;
        private readonly IEksStuffingGeneratorMk2 _eksStuffingGenerator;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ILogger _logger;
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
        private readonly EksPublishingJobDbContext _eksPublishingJobDbContext;

        public ExposureKeySetBatchJobMk3(
            IEksConfig eksConfig,
            IEksBuilder builder,
            EksPublishingJobDbContext eksPublishingJobDbContext,
            IUtcDateTimeProvider dateTimeProvider,
            ILogger<ExposureKeySetBatchJobMk3> logger,
            IEksStuffingGeneratorMk2 eksStuffingGenerator,
            ISnapshotEksInput snapshotter,
            MarkDiagnosisKeysAsUsedLocally markDiagnosisKeysAsUsed,
            IEksJobContentWriter contentWriter,
            IWriteStuffingToDiagnosisKeys writeStuffingToDiagnosisKeys)
        {
            _eksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _setBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _eksPublishingJobDbContext = eksPublishingJobDbContext ?? throw new ArgumentNullException(nameof(eksPublishingJobDbContext));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _eksStuffingGenerator = eksStuffingGenerator ?? throw new ArgumentNullException(nameof(eksStuffingGenerator));
            _snapshotter = snapshotter;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _markWorkFlowTeksAsUsed = markDiagnosisKeysAsUsed ?? throw new ArgumentNullException(nameof(markDiagnosisKeysAsUsed));
            _contentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
            _output = new List<EksCreateJobInputEntity>(_eksConfig.TekCountMax);
            _writeStuffingToDiagnosisKeys = writeStuffingToDiagnosisKeys ?? throw new ArgumentNullException(nameof(writeStuffingToDiagnosisKeys));
            _jobName = $"ExposureKeySetsJob_{_dateTimeProvider.Snapshot:u}".Replace(" ", "_").Replace(":", "_");
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            if (_fired)
            {
                throw new InvalidOperationException("One use only.");
            }

            _fired = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _logger.LogInformation("Started - JobName: {JobName}.", _jobName);

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

            _logger.LogInformation(
                "Reconciliation - Teks in EKSs matches usable input and stuffing - Delta: {ReconcileOutputCount}.",
                _eksEngineResult.ReconcileOutputCount);

            _logger.LogInformation(
                "Reconciliation - Teks in EKSs matches output count - Delta: {ReconcileEksSumCount}.",
                _eksEngineResult.ReconcileEksSumCount);

            _logger.LogInformation("{JobName} complete.", _jobName);

            return _eksEngineResult;
        }

        private async Task<int> GetTransmissionRiskNoneCountAsync()
        {
            return await _eksPublishingJobDbContext.EksInput.CountAsync(x => x.TransmissionRiskLevel == TransmissionRiskLevel.None);
        }

        private async Task ClearJobTablesAsync()
        {
            _logger.LogDebug("Clear job tables.");

            await _eksPublishingJobDbContext.TruncateAsync<EksCreateJobInputEntity>();
            await _eksPublishingJobDbContext.TruncateAsync<EksCreateJobOutputEntity>();
        }

        private async Task StuffAsync()
        {
            if (_eksEngineResult.InputCount == 0)
            {
                _logger.LogInformation("No stuffing required - No publishable TEKs.");
                return;
            }

            var tekCount = _eksPublishingJobDbContext.EksInput.Count(x => x.TransmissionRiskLevel != TransmissionRiskLevel.None);

            var stuffingCount = tekCount < _eksConfig.TekCountMin ? _eksConfig.TekCountMin - tekCount : 0;
            if (stuffingCount == 0)
            {
                _logger.LogInformation("No stuffing required - Minimum TEK count OK.");
                return;
            }

            _eksEngineResult.StuffingCount = stuffingCount;

            var stuffing = _eksStuffingGenerator.Execute(stuffingCount);
            _logger.LogInformation("Stuffing required - Count: {StuffingCount}.", stuffing.Length);

            await _eksPublishingJobDbContext.BulkInsertAsync(stuffing);
            _logger.LogInformation("Stuffing added.");
        }

        private async Task BuildOutputAsync()
        {
            _logger.LogDebug("Build EKSs.");
            _buildEksStopwatch.Start();

            var inputIndex = 0;
            var page = GetInputPage(inputIndex, _eksConfig.PageSize);
            _logger.LogDebug("Read TEKs - Count: {TeksReadCount}.", page.Length);

            while (page.Length > 0)
            {
                if (_output.Count + page.Length >= _eksConfig.TekCountMax)
                {
                    _logger.LogDebug("This page fills the EKS to capacity - Capacity: {Capacity}.",
                        _eksConfig.TekCountMax);

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
                _logger.LogDebug("Read TEKs - Count: {TeksReadCount}.", page.Length);
            }

            if (_output.Count > 0)
            {
                _logger.LogDebug("Write remaining TEKs - Count: {TeksRemainingCount}.", _output.Count);
                await WriteNewEksToOutputAsync();
            }
        }

        private void AddToOutput(EksCreateJobInputEntity[] page)
        {
            _output.AddRange(page); //Lots of memory
            _logger.LogDebug("Add TEKs to output - Count: {TeksAddedCount}, Total: {OutputCount}.",
                page.Length, _output.Count);
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
            _logger.LogDebug("Build EKS");

            var args = _output.Select(Map).ToArray();

            var content = await _setBuilder.BuildAsync(args);
            var release = _eksEngineResult.Started;
            var eksCount = ++_eksCount;

            // Generate a new guid string, formatted without dashes
            var newOutputId = Guid.NewGuid().ToString("N");

            var eksCreateJobOutputEntity = new EksCreateJobOutputEntity
            {
                Region = DefaultValues.Region,
                Release = release,
                CreatingJobQualifier = eksCount,
                Content = content,
                OutputId = newOutputId
            };

            _logger.LogInformation("Write EKS - Id: {CreatingJobQualifier}",
                eksCreateJobOutputEntity.CreatingJobQualifier);

            await using var tx = _eksPublishingJobDbContext.BeginTransaction();
            await _eksPublishingJobDbContext.AddAsync(eksCreateJobOutputEntity);
            _eksPublishingJobDbContext.SaveAndCommit();

            _logger.LogInformation("Mark TEKs as used");

            if (_output.Any())
            {
                var idsToUpdate = string.Join(",", _output.Select(x => x.Id.ToString()).ToArray());

                await _eksPublishingJobDbContext.BulkUpdateSqlRawAsync<EksCreateJobInputEntity>(
                    columnName: "used",
                    value: true,
                    ids: idsToUpdate);
            }

            _eksEngineResult.OutputCount += _output.Count;

            _eksResults.Add(new EksInfo { TekCount = _output.Count, TotalSeconds = _buildEksStopwatch.Elapsed.TotalSeconds });
            _output.Clear();
        }

        private EksCreateJobInputEntity[] GetInputPage(int skip, int take)
        {
            _logger.LogDebug("Read page - Skip {Skip}, Take {Take}", skip, take);

            var unFilteredResult = _eksPublishingJobDbContext.EksInput
                .AsNoTracking()
                .OrderBy(x => x.KeyData)
                .ThenBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToArray();

            _logger.LogDebug("Read page - Count: {EksReadCount}", unFilteredResult.Length);

            return unFilteredResult;
        }

        private async Task CommitResultsAsync()
        {
            _logger.LogInformation("Commit results - publish EKSs");

            await _contentWriter.ExecuteAsync();

            _logger.LogInformation("Commit results - Mark TEKs as Published");
            var result = await _markWorkFlowTeksAsUsed.ExecuteAsync();
            _logger.LogInformation("Marked as published - Total: {TotalMarked}", result.Marked);

            //Write stuffing to DKs
            _writeStuffingToDiagnosisKeys.Execute();

            await ClearJobTablesAsync();
        }
    }
}
