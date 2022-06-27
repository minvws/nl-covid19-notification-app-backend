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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    /// <summary>
    /// Build content
    /// </summary>
    public class IksEngine : BaseCommand
    {
        private readonly ILogger<IksEngine> _logger;
        private readonly IksInputSnapshotCommand _snapshotter;
        private readonly IksFormatter _formatter;

        private readonly IIksConfig _config;
        private readonly IUtcDateTimeProvider _dateTimeProvider;

        private readonly MarkDiagnosisKeysAsUsedByIks _markSourceAsUsed;

        private readonly IksJobContentWriter _contentWriter;

        private readonly string _jobName;
        private readonly IksEngineResult _engineResult = new IksEngineResult();
        private readonly IList<IksInfo> _iksResults = new List<IksInfo>();

        private readonly List<IksCreateJobInputEntity> _output = new List<IksCreateJobInputEntity>();
        private int _setCount;

        private bool _fired;
        private readonly Stopwatch _buildSetStopwatch = new Stopwatch();
        private readonly IksPublishingJobDbContext _publishingDbContext;


        public IksEngine(ILogger<IksEngine> logger, IksInputSnapshotCommand snapshotter, IksFormatter formatter, IIksConfig config, IUtcDateTimeProvider dateTimeProvider, MarkDiagnosisKeysAsUsedByIks markSourceAsUsed, IksJobContentWriter contentWriter, IksPublishingJobDbContext publishingDbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _snapshotter = snapshotter ?? throw new ArgumentNullException(nameof(snapshotter));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _markSourceAsUsed = markSourceAsUsed ?? throw new ArgumentNullException(nameof(markSourceAsUsed));
            _contentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
            _publishingDbContext = publishingDbContext ?? throw new ArgumentNullException(nameof(publishingDbContext));
            _jobName = "IksEngine";
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

            _logger.LogInformation("Started - JobName: {JobName}", _jobName);
            
            _engineResult.Started = _dateTimeProvider.Snapshot; //Align with the logged job name.

            await ClearJobTables();

            var snapshotResult = _snapshotter.Execute();

            _engineResult.InputCount = snapshotResult.Count;
            _engineResult.SnapshotSeconds = snapshotResult.ElapsedSeconds;

            if (snapshotResult.Count != 0)
            {
                await BuildOutput();
                await CommitResults();
            }

            _engineResult.TotalSeconds = stopwatch.Elapsed.TotalSeconds;
            _engineResult.Items = _iksResults.ToArray();

            _logger.LogInformation("Reconciliation - Teks in EKSs matches usable input and stuffing - Delta: {ReconcileOutputCount}", _engineResult.ReconcileOutputCount);
            _logger.LogInformation("Reconciliation - Teks in EKSs matches output count - Delta: {ReconcileEksSumCount}", _engineResult.ReconcileEksSumCount);

            _logger.LogInformation("{JobName} complete.", _jobName);

            return _engineResult;
        }

        private async Task ClearJobTables()
        {
            _logger.LogDebug("Clear job tables.");

            await _publishingDbContext.TruncateAsync<IksCreateJobInputEntity>();
            await _publishingDbContext.TruncateAsync<IksCreateJobOutputEntity>();
        }

        private async Task BuildOutput()
        {
            _logger.LogDebug("Build EKSs.");
            _buildSetStopwatch.Start();

            var inputIndex = 0;
            var page = GetInputPage(inputIndex, _config.PageSize);
            _logger.LogDebug("Read TEKs - Count: {Count}", page.Length);

            while (page.Length > 0)
            {
                if (_output.Count + page.Length >= _config.ItemCountMax)
                {
                    _logger.LogDebug("This page fills the EKS to capacity - Capacity: {Capacity}.", _config.ItemCountMax);
                    var remainingCapacity = _config.ItemCountMax - _output.Count;
                    AddToOutput(page.Take(remainingCapacity).ToArray()); //Fill to max
                    await WriteNewSetToOutput();
                    AddToOutput(page.Skip(remainingCapacity).ToArray()); //Use leftovers from the page
                }
                else
                {
                    AddToOutput(page);
                }

                inputIndex += _config.PageSize; //Move input index
                page = GetInputPage(inputIndex, _config.PageSize); //Read next page.
                _logger.LogDebug("Read TEKs - Count: {Count}.", page.Length);
            }

            if (_output.Count > 0)
            {
                _logger.LogDebug("Write remaining TEKs - Count: {Count}.", _output.Count);
                await WriteNewSetToOutput();
            }
        }


        private void AddToOutput(IksCreateJobInputEntity[] page)
        {
            _output.AddRange(page); //Lots of memory
            _logger.LogDebug("Add TEKs to output - Count: {Count}, Total: {OutputCount}.", page.Length, _output.Count);
        }

        private async Task WriteNewSetToOutput()
        {
            _logger.LogDebug("Build IKS.");

            var args = _output.Select(MappingDefaults.ToInteropKeyFormatterArgs).ToArray();

            var content = _formatter.Format(args);

            var e = new IksCreateJobOutputEntity
            {
                Created = _engineResult.Started,
                CreatingJobQualifier = ++_setCount,
                Content = content
            };

            _logger.LogInformation("Write IKS - Id: {CreatingJobQualifier}.", e.CreatingJobQualifier);

            await using var tx = _publishingDbContext.BeginTransaction();
            await _publishingDbContext.AddAsync(e);
            _publishingDbContext.SaveAndCommit();

            _logger.LogInformation("Mark TEKs as used.");

            //Could be 750k in this hit

            var idsToUpdate = string.Join(",", _output.Select(x => x.Id.ToString()).ToArray());

            await _publishingDbContext.BulkUpdateSqlRawAsync(
                tableName: "IksCreateJobInput",
                columnName: "Used",
                value: true,
                ids: idsToUpdate);

            _engineResult.OutputCount += _output.Count;

            _iksResults.Add(new IksInfo { ItemCount = _output.Count, TotalSeconds = _buildSetStopwatch.Elapsed.TotalSeconds });
            _output.Clear();
        }

        private IksCreateJobInputEntity[] GetInputPage(int skip, int take)
        {
            _logger.LogDebug("Read page - Skip {Skip}, Take {Take}.", skip, take);

            var result = _publishingDbContext.Input
                .AsNoTracking()
                .OrderBy(x => x.DailyKey.KeyData)
                .Skip(skip)
                .Take(take)
                .ToArray();

            _logger.LogDebug("Read page - Count: {Count}.", result.Length);

            return result;
        }

        private async Task CommitResults()
        {
            _logger.LogInformation("Commit results - publish EKSs.");

            await _contentWriter.ExecuteAsync();

            _logger.LogInformation("Commit results - Mark TEKs as Published.");
            var result = await _markSourceAsUsed.ExecuteAsync();
            _logger.LogInformation("Marked as published - Total: {TotalMarked}.", result);

            await ClearJobTables();
        }
    }
}
