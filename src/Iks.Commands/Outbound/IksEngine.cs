// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
    public class IksEngine
    {
        private readonly IWrappedEfExtensions _sqlCommands;

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
        private readonly Func<IksPublishingJobDbContext> _publishingDbContextFac;


        public IksEngine(ILogger<IksEngine> logger, IksInputSnapshotCommand snapshotter, IksFormatter formatter, IIksConfig config, IUtcDateTimeProvider dateTimeProvider, MarkDiagnosisKeysAsUsedByIks markSourceAsUsed, IksJobContentWriter contentWriter, Func<IksPublishingJobDbContext> publishingDbContextFac, IWrappedEfExtensions sqlCommands)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _snapshotter = snapshotter ?? throw new ArgumentNullException(nameof(snapshotter));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _markSourceAsUsed = markSourceAsUsed ?? throw new ArgumentNullException(nameof(markSourceAsUsed));
            _contentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
            _publishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _sqlCommands = sqlCommands ?? throw new ArgumentNullException(nameof(sqlCommands));
            _jobName = "IksEngine";
        }

        public async Task<IksEngineResult> ExecuteAsync()
        {
            if (_fired)
                throw new InvalidOperationException("One use only.");

            _fired = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _logger.LogInformation("Started - JobName:{JobName}", _jobName);

            if (Environment.UserInteractive && !WindowsIdentityQueries.CurrentUserIsAdministrator())
                _logger.LogWarning("{JobName} started WITHOUT elevated privileges - errors may occur when signing content.", _jobName);

            _engineResult.Started = _dateTimeProvider.Snapshot; //Align with the logged job name.

            await ClearJobTables();

            var snapshotResult = await _snapshotter.ExecuteAsync();

            _engineResult.InputCount = snapshotResult.Count;
            _engineResult.SnapshotSeconds = snapshotResult.ElapsedSeconds;

            if (snapshotResult.Count != 0)
            {
                await BuildOutput();
                await CommitResults();
            }

            _engineResult.TotalSeconds = stopwatch.Elapsed.TotalSeconds;
            _engineResult.Items = _iksResults.ToArray();

            _logger.LogInformation("Reconciliation - Teks in EKSs matches usable input and stuffing - Delta:{ReconcileOutputCount}", _engineResult.ReconcileOutputCount);
            _logger.LogInformation("Reconciliation - Teks in EKSs matches output count - Delta:{ReconcileEksSumCount}", _engineResult.ReconcileEksSumCount);

            _logger.LogInformation("{JobName} complete.", _jobName);

            return _engineResult;
        }

        private async Task ClearJobTables()
        {
            _logger.LogDebug("Clear job tables.");

            await using var dbc = _publishingDbContextFac();
            await _sqlCommands.TruncateTableAsync(dbc, TableNames.IksEngineInput);
            await _sqlCommands.TruncateTableAsync(dbc, TableNames.IksEngineOutput);
        }

        private async Task BuildOutput()
        {
            _logger.LogDebug("Build EKSs.");
            _buildSetStopwatch.Start();

            var inputIndex = 0;
            var page = GetInputPage(inputIndex, _config.PageSize);
            _logger.LogDebug("Read TEKs - Count:{Count}", page.Length);

            while (page.Length > 0)
            {
                if (_output.Count + page.Length >= _config.ItemCountMax)
                {
                    _logger.LogDebug("This page fills the EKS to capacity - Capacity:{Capacity}.", _config.ItemCountMax);
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
                _logger.LogDebug("Read TEKs - Count:{Count}.", page.Length);
            }

            if (_output.Count > 0)
            {
                _logger.LogDebug("Write remaining TEKs - Count:{Count}.", _output.Count);
                await WriteNewSetToOutput();
            }
        }


        private void AddToOutput(IksCreateJobInputEntity[] page)
        {
            _output.AddRange(page); //Lots of memory
            _logger.LogDebug("Add TEKs to output - Count:{Count}, Total:{OutputCount}.", page.Length, _output.Count);
        }


        //TODO make this a writer
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

            _logger.LogInformation("Write IKS - Id:{CreatingJobQualifier}.", e.CreatingJobQualifier);

            await using (var dbc = _publishingDbContextFac())
            {
                await using var tx = dbc.BeginTransaction();
                await dbc.AddAsync(e);
                dbc.SaveAndCommit();
            }

            _logger.LogInformation("Mark TEKs as used.");

            foreach (var i in _output)
                i.Used = true;

            //Could be 750k in this hit
            await using (var dbc2 = _publishingDbContextFac())
            {
                var bargs = new SubsetBulkArgs
                {
                    PropertiesToInclude = new[] { nameof(IksCreateJobInputEntity.Used) }
                };
                await dbc2.BulkUpdateAsync2(_output, bargs); //TX
            }

            _engineResult.OutputCount += _output.Count;

            _iksResults.Add(new IksInfo { ItemCount = _output.Count, TotalSeconds = _buildSetStopwatch.Elapsed.TotalSeconds });
            _output.Clear();
        }

        private IksCreateJobInputEntity[] GetInputPage(int skip, int take)
        {
            _logger.LogDebug("Read page - Skip {Skip}, Take {Take}.", skip, take);

            using var dbc = _publishingDbContextFac();
            var result = dbc.Set<IksCreateJobInputEntity>()
                .OrderBy(x => x.DailyKey.KeyData)
                .Skip(skip)
                .Take(take)
                .ToArray();

            _logger.LogDebug("Read page - Count:{Count}.", result.Length);

            return result;
        }

        private async Task CommitResults()
        {
            _logger.LogInformation("Commit results - publish EKSs.");

            await _contentWriter.ExecuteAsyc();

            _logger.LogInformation("Commit results - Mark TEKs as Published.");
            var result = await _markSourceAsUsed.ExecuteAsync();
            _logger.LogInformation("Marked as published - Total:{TotalMarked}.", result);

            await ClearJobTables();
        }
    }
}
