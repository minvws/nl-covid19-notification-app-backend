// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;
//using Eu.Interop;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Interoperability.DbContexts;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Interoperability.Entities;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Interoperability.IksInbound;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Interoperability.IksOutbound;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Efgs.Tests
//{
//    public class IksEngineBatchJob
//    {
//        private readonly string _JobName;
//        private readonly IksEngineResult _Result = new IksEngineResult();
//        private readonly IList<IksInfo> _IksResults = new List<IksInfo>();
//        private int _EksCount;

//        private readonly IWrappedEfExtensions _SqlCommands;
//        private readonly IIksConfig _IksConfig;
//        private readonly IUtcDateTimeProvider _DateTimeProvider;

//        private readonly IksImportBatchJob _ImportIks;
//        private readonly IksInputSnapshotCommand _Snapshotter;
//        private readonly IksFormatter _SetBuilder;
//        //private readonly IksEngineLoggingExtensions _Logger;
//        private readonly MarkDiagnosisKeysAsUsedByIks _MarkWorkFlowTeksAsUsed;

//        //Commit results
//        private readonly IksJobContentWriter _ContentWriter;

//        private readonly List<IksCreateJobInputEntity> _Output;

//        private bool _Fired;
//        private readonly Stopwatch _BuildEksStopwatch = new Stopwatch();
//        private readonly Func<IksPublishingJobDbContext> _PublishingDbContextFac;

//        public async Task<IksEngineResult> ExecuteAsync()
//        {
//            if (_Fired)
//                throw new InvalidOperationException("One use only.");

//            _Fired = true;

//            var stopwatch = new Stopwatch();
//            stopwatch.Start();

//            //_Logger.WriteStart(_JobName);

//            //if (Environment.UserInteractive && !WindowsIdentityQueries.CurrentUserIsAdministrator())
//            //    _Logger.WriteNoElevatedPrivs(_JobName);

//            await _ImportIks.ExecuteAsync();

//            _Result.Started = _DateTimeProvider.Snapshot; //Align with the logged job name.

//            await ClearJobTablesAsync();
//            var snapshotResult = await _Snapshotter.ExecuteAsync();

//            _Result.InputCount = snapshotResult.Count;
//            _Result.SnapshotSeconds = snapshotResult.ElapsedSeconds;

//            if (snapshotResult.Count != 0)
//            {
//                await BuildOutputAsync();
//                await CommitResultsAsync();
//            }

//            _Result.TotalSeconds = stopwatch.Elapsed.TotalSeconds;
//            _Result.Items = _IksResults.ToArray();

//            //_Logger.WriteReconciliationMatchUsable(_Result.ReconcileOutputCount);
//            //_Logger.WriteReconciliationMatchCount(_Result.ReconcileEksSumCount);
//            //_Logger.WriteFinished(_JobName);

//            return _Result;
//        }

//        private async Task ClearJobTablesAsync()
//        {
//            //_Logger.WriteCleartables();

//            await using var dbc = _PublishingDbContextFac();
//            _SqlCommands.TruncateTable(dbc, TableNames.IksEngineInput);
//            _SqlCommands.TruncateTable(dbc, TableNames.IksEngineOutput);
//        }

//        private async Task BuildOutputAsync()
//        {
//            //_Logger.WriteBuildEkses();
//            _BuildEksStopwatch.Start();

//            var inputIndex = 0;
//            var page = GetInputPage(inputIndex, _IksConfig.PageSize);
//            //_Logger.WriteReadTeks(page.Length);

//            while (page.Length > 0)
//            {
//                if (_Output.Count + page.Length >= _IksConfig.ItemCountMax)
//                {
//                    //_Logger.WritePageFillsToCapacity(_EksConfig.TekCountMax);
//                    var remainingCapacity = _IksConfig.ItemCountMax - _Output.Count;
//                    AddToOutput(page.Take(remainingCapacity).ToArray()); //Fill to max
//                    await WriteNewEksToOutputAsync();
//                    AddToOutput(page.Skip(remainingCapacity).ToArray()); //Use leftovers from the page
//                }
//                else
//                {
//                    AddToOutput(page);
//                }

//                inputIndex += _IksConfig.PageSize; //Move input index
//                page = GetInputPage(inputIndex, _IksConfig.PageSize); //Read next page.
//                //_Logger.WriteReadTeks(page.Length);
//            }

//            if (_Output.Count > 0)
//            {
//                //_Logger.WriteRemainingTeks(_Output.Count);
//                await WriteNewEksToOutputAsync();
//            }
//        }

//        private void AddToOutput(IksCreateJobInputEntity[] page)
//        {
//            _Output.AddRange(page); //Lots of memory
//            //_Logger.WriteAddTeksToOutput(page.Length, _Output.Count);
//        }

//        private static InteropKeyFormatterArgs Map(IksCreateJobInputEntity c)
//            => new InteropKeyFormatterArgs
//            {
//                Value = c.DailyKey,
//                TransmissionRiskLevel = (int)c.TransmissionRiskLevel,
//                CountriesOfInterest = c.CountriesOfInterest.Split(",", StringSplitOptions.RemoveEmptyEntries), //TODO values here or processor
//                DaysSinceSymtpomsOnset = c.DaysSinceSymptomsOnset,
//                ReportType = (EfgsReportType)c.ReportType, //TODO mapper/processor?
//            };

//        private async Task WriteNewEksToOutputAsync()
//        {
//            //_Logger.WriteBuildEntry();

//            var args = _Output.Select(Map).ToArray();

//            var content = _SetBuilder.Format(args);

//            var e = new IksCreateJobOutputEntity
//            {
//                Created = _Result.Started,
//                CreatingJobQualifier = ++_EksCount,
//                Content = content,
//            };

//            //_Logger.WriteWritingCurrentEks(e.CreatingJobQualifier);

//            await using (var dbc = _PublishingDbContextFac())
//            {
//                await using var tx = dbc.BeginTransaction();
//                await dbc.AddAsync(e);
//                dbc.SaveAndCommit();
//            }

//            //_Logger.WriteMarkTekAsUsed();

//            foreach (var i in _Output)
//                i.Used = true;

//            //Could be 750k in this hit
//            await using (var dbc2 = _PublishingDbContextFac())
//            {
//                var bargs = new SubsetBulkArgs
//                {
//                    PropertiesToInclude = new[] { nameof(EksCreateJobInputEntity.Used) }
//                };
//                await dbc2.BulkUpdateAsync2(_Output, bargs); //TX
//            }

//            _Result.OutputCount += _Output.Count;

//            _IksResults.Add(new IksInfo { ItemCount = _Output.Count, TotalSeconds = _BuildEksStopwatch.Elapsed.TotalSeconds });
//            _Output.Clear();
//        }

//        private IksCreateJobInputEntity[] GetInputPage(int skip, int take)
//        {
//            //_Logger.WriteStartReadPage(skip, take);

//            using var dbc = _PublishingDbContextFac();
//            var result = dbc.Set<IksCreateJobInputEntity>()
//                .Where(x => x.TransmissionRiskLevel != TransmissionRiskLevel.None)
//                .OrderBy(x => x.DailyKey.KeyData).Skip(skip).Take(take).ToArray();

//            //_Logger.WriteFinishedReadPage(result.Length);

//            return result;
//        }

//        private async Task CommitResultsAsync()
//        {
//            //_Logger.WriteCommitPublish();

//            await _ContentWriter.ExecuteAsyc();

//            //_Logger.WriteCommitMarkTeks();
//            var result = await _MarkWorkFlowTeksAsUsed.ExecuteAsync();
//            //_Logger.WriteTotalMarked(result.Marked);

//            await ClearJobTablesAsync();
//        }
//    }
//}