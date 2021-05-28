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
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands
{
    public class SnapshotWorkflowTeksToDksCommand
    {
        private readonly ILogger<SnapshotWorkflowTeksToDksCommand> _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ITransmissionRiskLevelCalculationMk2 _TransmissionRiskLevelCalculation;
        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly Func<WorkflowDbContext> _WorkflowDbContextFactory;
        private readonly Func<DkSourceDbContext> _DkSourceDbContextFactory;
        private readonly IWrappedEfExtensions _SqlCommands;
        private readonly IDiagnosticKeyProcessor[] _OrderedProcessorList;

        private int _CommitIndex;
        private SnapshotWorkflowTeksToDksResult _Result;

        public SnapshotWorkflowTeksToDksCommand(ILogger<SnapshotWorkflowTeksToDksCommand> logger, IUtcDateTimeProvider dateTimeProvider, ITransmissionRiskLevelCalculationMk2 transmissionRiskLevelCalculation, WorkflowDbContext workflowDbContext, Func<WorkflowDbContext> workflowDbContextFactory, Func<DkSourceDbContext> dkSourceDbContextFactory, IWrappedEfExtensions sqlCommands, IDiagnosticKeyProcessor[] orderedProcessorList)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _TransmissionRiskLevelCalculation = transmissionRiskLevelCalculation ?? throw new ArgumentNullException(nameof(transmissionRiskLevelCalculation));
            _WorkflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _WorkflowDbContextFactory = workflowDbContextFactory ?? throw new ArgumentNullException(nameof(workflowDbContextFactory));
            _DkSourceDbContextFactory = dkSourceDbContextFactory ?? throw new ArgumentNullException(nameof(dkSourceDbContextFactory));
            _SqlCommands = sqlCommands ?? throw new ArgumentNullException(nameof(sqlCommands));
            _OrderedProcessorList = orderedProcessorList ?? throw new ArgumentNullException(nameof(orderedProcessorList));
        }

        public async Task<SnapshotWorkflowTeksToDksResult> ExecuteAsync()
        {
            if (_Result != null)
                throw new InvalidOperationException();

            _Result = new SnapshotWorkflowTeksToDksResult();
            await ClearJobTablesAsync();
            await SnapshotTeks();
            await CommitSnapshotAsync();
            return _Result;
        }

        private async Task ClearJobTablesAsync()
        {
            var dbc = _DkSourceDbContextFactory();
            await _SqlCommands.TruncateTableAsync(dbc, TableNames.DiagnosisKeysInput);
        }

        private async Task SnapshotTeks()
        {
            _Logger.LogDebug("Snapshot publishable TEKs.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int pagesize = 10000;
            var index = 0;

            using var tx = _WorkflowDbContext.BeginTransaction();
            var page = ReadFromWorkflow(index, pagesize);

            while (page.Count > 0)
            {
                var db = _DkSourceDbContextFactory();
                await db.BulkInsertAsync2(page.ToList(), new SubsetBulkArgs());

                index += page.Count;
                page = ReadFromWorkflow(index, pagesize);
            }

            _Result.TekReadCount = index;
        }

        private IList<DiagnosisKeyInputEntity> ReadFromWorkflow(int index, int pageSize)
        {
            var snapshot = _DateTimeProvider.Snapshot;

            // Select TemporaryExposureKeys from Workflow table which are ready to be processed (TEK is: AuthorisedByCaregiver, StartDateOfTekInclusion and IsSymptomatic are set, Unpublished PublishAfter not in the future)
            var selectTeksFromWorkflowQuery = _WorkflowDbContext.TemporaryExposureKeys
                .Where(x => x.Owner.AuthorisedByCaregiver != null
                            && x.Owner.StartDateOfTekInclusion != null
                            && x.PublishingState == PublishingState.Unpublished
                            && x.PublishAfter <= snapshot
                            && x.Owner.IsSymptomatic.HasValue
                )
                .Skip(index)
                .Take(pageSize)
                .Select(x => new {
                    x.Id,
                    DailyKey = new DailyKey(x.KeyData, x.RollingStartNumber, UniversalConstants.RollingPeriodRange.Hi), //Constant cos iOS xxx requires all RP to be 144
                    DateOfSymptomsOnset = x.Owner.StartDateOfTekInclusion.Value,
                    Symptomatic = x.Owner.IsSymptomatic.Value
                }).ToList();

            // Map TEKS from selectTeksFromWorkflowQuery to a List of DiagnosisKeyInputEntities
            var diagnosisKeyInputEntities = selectTeksFromWorkflowQuery.Select(x =>
                {
                    var dsos = x.DailyKey.RollingStartNumber.DaysSinceSymptomOnset(x.DateOfSymptomsOnset);
                    var trl = _TransmissionRiskLevelCalculation.Calculate(dsos);
                    var result = new DiagnosisKeyInputEntity
                    {
                        DailyKey = x.DailyKey,
                        TekId = x.Id,
                        Local = new LocalTekInfo 
                        {
                            DaysSinceSymptomsOnset = dsos, //Added here as the new format has this as well as the EFGS format.
                            TransmissionRiskLevel = trl,
                            Symptomatic = x.Symptomatic
                        }
                    };
                    return result;
                }).ToList();

          
            return diagnosisKeyInputEntities;
        }

        private async Task CommitSnapshotAsync()
        {
            var used = ReadDkPage();
            while (used.Length > 0)
            {
                await WriteToDksAsync(used);
                await MarkTeksAsPublishedAsync(used.Select(x => x.TekId).ToArray());
                used = ReadDkPage();
            }
        }

        private async Task WriteToDksAsync(DiagnosisKeyInputEntity[] used)
        {
            await using var db = _DkSourceDbContextFactory();

            var q3 = used.Select(x => (DkProcessingItem?)new DkProcessingItem
            {
                DiagnosisKey = new DiagnosisKeyEntity
                {
                    DailyKey = x.DailyKey,
                    Local = x.Local,
                    Origin = TekOrigin.Local,
                },
                Metadata = new Dictionary<string, object>
                {
                    //Depends on complex filtering requiring intermediate results to be communicated between filters.
                }
            }).ToArray();

            var q4 = _OrderedProcessorList.Execute(q3);
            var items = q4.Select(x => x.DiagnosisKey).ToList();
            _Result.DkCount += items.Count;

            await db.BulkInsertAsync2(items, new SubsetBulkArgs());
        }

        private async Task MarkTeksAsPublishedAsync(long[] used)
        {
            await using var wfDb = _WorkflowDbContextFactory();

            var zap = wfDb.TemporaryExposureKeys
                .Where(x => used.Contains(x.Id))
                .ToList();

            _CommitIndex += used.Length;
            _Logger.LogInformation("Marking TEKs as Published - Count:{Count}, Running total:{RunningTotal}.", zap.Count, _CommitIndex);

            foreach (var i in zap)
            {
                i.PublishingState = PublishingState.Published;
            }

            var bargs = new SubsetBulkArgs
            {
                PropertiesToInclude = new[] { $"{nameof(TekEntity.PublishingState)}" }
            };

            await wfDb.BulkUpdateAsync2(zap, bargs); //TX
        }

        private DiagnosisKeyInputEntity[] ReadDkPage()
        {
            var q = _DkSourceDbContextFactory().DiagnosisKeysInput
                .OrderBy(x => x.TekId)
                .Skip(_CommitIndex)
                .Take(1000)
                .ToArray();

            return q.ToArray();
        }
    }
}