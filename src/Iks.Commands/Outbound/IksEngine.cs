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
        private readonly IWrappedEfExtensions _SqlCommands;

        private readonly ILogger<IksEngine> _Logger;
        private readonly IksInputSnapshotCommand _Snapshotter;
        private readonly IksFormatter _Formatter;

        private readonly IIksConfig _Config;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        private readonly MarkDiagnosisKeysAsUsedByIks _MarkSourceAsUsed;

        private readonly IksJobContentWriter _ContentWriter;

        private readonly string _JobName;
        private readonly IksEngineResult _EngineResult = new IksEngineResult();
        private readonly IList<IksInfo> _IksResults = new List<IksInfo>();

        private readonly List<IksCreateJobInputEntity> _Output = new List<IksCreateJobInputEntity>();
        private int _SetCount;

        private bool _Fired;
        private readonly Stopwatch _BuildSetStopwatch = new Stopwatch();
        private readonly Func<IksPublishingJobDbContext> _PublishingDbContextFac;


        public IksEngine(ILogger<IksEngine> logger, IksInputSnapshotCommand snapshotter, IksFormatter formatter, IIksConfig config, IUtcDateTimeProvider dateTimeProvider, MarkDiagnosisKeysAsUsedByIks markSourceAsUsed, IksJobContentWriter contentWriter, Func<IksPublishingJobDbContext> publishingDbContextFac, IWrappedEfExtensions sqlCommands)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Snapshotter = snapshotter ?? throw new ArgumentNullException(nameof(snapshotter));
            _Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _MarkSourceAsUsed = markSourceAsUsed ?? throw new ArgumentNullException(nameof(markSourceAsUsed));
            _ContentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
            _PublishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _SqlCommands = sqlCommands ?? throw new ArgumentNullException(nameof(sqlCommands));
            _JobName = "IksEngine";
        }

        public async Task<IksEngineResult> ExecuteAsync()
        {
            if (_Fired)
                throw new InvalidOperationException("One use only.");

            _Fired = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _Logger.LogInformation("Started - JobName:{JobName}", _JobName);

            if (Environment.UserInteractive && !WindowsIdentityQueries.CurrentUserIsAdministrator())
                _Logger.LogWarning("{JobName} started WITHOUT elevated privileges - errors may occur when signing content.", _JobName);

            _EngineResult.Started = _DateTimeProvider.Snapshot; //Align with the logged job name.

            await ClearJobTables();

            var snapshotResult = await _Snapshotter.ExecuteAsync();

            _EngineResult.InputCount = snapshotResult.Count;
            _EngineResult.SnapshotSeconds = snapshotResult.ElapsedSeconds;

            if (snapshotResult.Count != 0)
            {
                await BuildOutput();
                await CommitResults();
            }

            _EngineResult.TotalSeconds = stopwatch.Elapsed.TotalSeconds;
            _EngineResult.Items = _IksResults.ToArray();

            _Logger.LogInformation("Reconciliation - Teks in EKSs matches usable input and stuffing - Delta:{ReconcileOutputCount}", _EngineResult.ReconcileOutputCount);
            _Logger.LogInformation("Reconciliation - Teks in EKSs matches output count - Delta:{ReconcileEksSumCount}", _EngineResult.ReconcileEksSumCount);

            _Logger.LogInformation("{JobName} complete.", _JobName);

            return _EngineResult;
        }

        private async Task ClearJobTables()
        {
            _Logger.LogDebug("Clear job tables.");

            await using var dbc = _PublishingDbContextFac();
            await _SqlCommands.TruncateTableAsync(dbc, TableNames.IksEngineInput);
            await _SqlCommands.TruncateTableAsync(dbc, TableNames.IksEngineOutput);
        }

        private async Task BuildOutput()
        {
            _Logger.LogDebug("Build EKSs.");
            _BuildSetStopwatch.Start();

            var inputIndex = 0;
            var page = GetInputPage(inputIndex, _Config.PageSize);
            _Logger.LogDebug("Read TEKs - Count:{Count}", page.Length);

            while (page.Length > 0)
            {
                if (_Output.Count + page.Length >= _Config.ItemCountMax)
                {
                    _Logger.LogDebug("This page fills the EKS to capacity - Capacity:{Capacity}.", _Config.ItemCountMax);
                    var remainingCapacity = _Config.ItemCountMax - _Output.Count;
                    AddToOutput(page.Take(remainingCapacity).ToArray()); //Fill to max
                    await WriteNewSetToOutput();
                    AddToOutput(page.Skip(remainingCapacity).ToArray()); //Use leftovers from the page
                }
                else
                {
                    AddToOutput(page);
                }

                inputIndex += _Config.PageSize; //Move input index
                page = GetInputPage(inputIndex, _Config.PageSize); //Read next page.
                _Logger.LogDebug("Read TEKs - Count:{Count}.", page.Length);
            }

            if (_Output.Count > 0)
            {
                _Logger.LogDebug("Write remaining TEKs - Count:{Count}.", _Output.Count);
                await WriteNewSetToOutput();
            }
        }


        private void AddToOutput(IksCreateJobInputEntity[] page)
        {
            _Output.AddRange(page); //Lots of memory
            _Logger.LogDebug("Add TEKs to output - Count:{Count}, Total:{OutputCount}.", page.Length, _Output.Count);
        }


        //TODO make this a writer
        private async Task WriteNewSetToOutput()
        {
            _Logger.LogDebug("Build IKS.");

            var args = _Output.Select(MappingDefaults.ToInteropKeyFormatterArgs).ToArray();

            var content = _Formatter.Format(args);

            var e = new IksCreateJobOutputEntity
            {
                Created = _EngineResult.Started,
                CreatingJobQualifier = ++_SetCount,
                Content = content
            };

            _Logger.LogInformation("Write IKS - Id:{CreatingJobQualifier}.", e.CreatingJobQualifier);

            await using (var dbc = _PublishingDbContextFac())
            {
                await using var tx = dbc.BeginTransaction();
                await dbc.AddAsync(e);
                dbc.SaveAndCommit();
            }

            _Logger.LogInformation("Mark TEKs as used.");

            foreach (var i in _Output)
                i.Used = true;

            //Could be 750k in this hit
            await using (var dbc2 = _PublishingDbContextFac())
            {
                var bargs = new SubsetBulkArgs
                {
                    PropertiesToInclude = new[] { nameof(IksCreateJobInputEntity.Used) }
                };
                await dbc2.BulkUpdateAsync2(_Output, bargs); //TX
            }

            _EngineResult.OutputCount += _Output.Count;

            _IksResults.Add(new IksInfo { ItemCount = _Output.Count, TotalSeconds = _BuildSetStopwatch.Elapsed.TotalSeconds });
            _Output.Clear();
        }

        private IksCreateJobInputEntity[] GetInputPage(int skip, int take)
        {
            _Logger.LogDebug("Read page - Skip {Skip}, Take {Take}.", skip, take);

            using var dbc = _PublishingDbContextFac();
            var result = dbc.Set<IksCreateJobInputEntity>()
                .OrderBy(x => x.DailyKey.KeyData)
                .Skip(skip)
                .Take(take)
                .ToArray();

            _Logger.LogDebug("Read page - Count:{Count}.", result.Length);

            return result;
        }

        private async Task CommitResults()
        {
            _Logger.LogInformation("Commit results - publish EKSs.");

            await _ContentWriter.ExecuteAsyc();

            _Logger.LogInformation("Commit results - Mark TEKs as Published.");
            var result = await _MarkSourceAsUsed.ExecuteAsync();
            _Logger.LogInformation("Marked as published - Total:{TotalMarked}.", result);

            await ClearJobTables();
        }
    }
}
