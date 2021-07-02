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
        private readonly ILogger<SnapshotWorkflowTeksToDksCommand> _logger;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ITransmissionRiskLevelCalculationMk2 _transmissionRiskLevelCalculation;
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly DkSourceDbContext _dkSourceDbContext;
        private readonly IWrappedEfExtensions _sqlCommands;
        private readonly IDiagnosticKeyProcessor[] _orderedProcessorList;

        private int _commitIndex;
        private SnapshotWorkflowTeksToDksResult _result;

        public SnapshotWorkflowTeksToDksCommand(ILogger<SnapshotWorkflowTeksToDksCommand> logger, IUtcDateTimeProvider dateTimeProvider, ITransmissionRiskLevelCalculationMk2 transmissionRiskLevelCalculation, WorkflowDbContext workflowDbContext, DkSourceDbContext dkSourceDbContext, IWrappedEfExtensions sqlCommands, IDiagnosticKeyProcessor[] orderedProcessorList)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _transmissionRiskLevelCalculation = transmissionRiskLevelCalculation ?? throw new ArgumentNullException(nameof(transmissionRiskLevelCalculation));
            _workflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
            _sqlCommands = sqlCommands ?? throw new ArgumentNullException(nameof(sqlCommands));
            _orderedProcessorList = orderedProcessorList ?? throw new ArgumentNullException(nameof(orderedProcessorList));
        }

        public async Task<SnapshotWorkflowTeksToDksResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException();
            }

            _result = new SnapshotWorkflowTeksToDksResult();
            await ClearJobTablesAsync();
            await SnapshotTeks();
            await CommitSnapshotAsync();
            return _result;
        }

        private async Task ClearJobTablesAsync()
        {
            await _sqlCommands.TruncateTableAsync(_dkSourceDbContext, TableNames.DiagnosisKeysInput);
        }

        private async Task SnapshotTeks()
        {
            _logger.LogDebug("Snapshot publishable TEKs.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int PageSize = 10000;
            var index = 0;

            var page = ReadFromWorkflow(index, PageSize);

            while (page.Count > 0)
            {
                await _dkSourceDbContext.BulkInsertAsync2(page.ToList(), new SubsetBulkArgs());

                index += page.Count;
                page = ReadFromWorkflow(index, PageSize);
            }

            _result.TekReadCount = index;
        }

        private IList<DiagnosisKeyInputEntity> ReadFromWorkflow(int index, int pageSize)
        {
            var snapshot = _dateTimeProvider.Snapshot;

            // Select TemporaryExposureKeys from Workflow table which are ready to be processed (TEK is: AuthorisedByCaregiver, StartDateOfTekInclusion and IsSymptomatic are set, Unpublished PublishAfter not in the future)
            var selectTeksFromWorkflowQuery = _workflowDbContext.TemporaryExposureKeys
                .AsNoTracking()
                .Where(x => x.Owner.AuthorisedByCaregiver != null
                            && x.Owner.StartDateOfTekInclusion != null
                            && x.PublishingState == PublishingState.Unpublished
                            && x.PublishAfter <= snapshot
                            && x.Owner.IsSymptomatic.HasValue
                )
                .Skip(index)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    DailyKey = new DailyKey(x.KeyData, x.RollingStartNumber, UniversalConstants.RollingPeriodRange.Hi), //Constant cos iOS xxx requires all RP to be 144
                    DateOfSymptomsOnset = x.Owner.StartDateOfTekInclusion.Value,
                    Symptomatic = x.Owner.IsSymptomatic.Value
                }).ToList();

            // Map TEKS from selectTeksFromWorkflowQuery to a List of DiagnosisKeyInputEntities
            var diagnosisKeyInputEntities = selectTeksFromWorkflowQuery.Select(x =>
                {
                    var dsos = x.DailyKey.RollingStartNumber.DaysSinceSymptomOnset(x.DateOfSymptomsOnset);
                    var trl = _transmissionRiskLevelCalculation.Calculate(dsos);
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
            var q3 = used.Select(x => (DkProcessingItem)new DkProcessingItem
            {
                DiagnosisKey = new DiagnosisKeyEntity
                {
                    DailyKey = x.DailyKey,
                    Local = x.Local,
                    Origin = TekOrigin.Local,
                    Created = DateTime.Now
                },
                Metadata = new Dictionary<string, object>
                {
                    //Depends on complex filtering requiring intermediate results to be communicated between filters.
                }
            }).ToArray();

            var q4 = _orderedProcessorList.Execute(q3);
            var items = q4.Select(x => x.DiagnosisKey).ToList();
            _result.DkCount += items.Count;

            await _dkSourceDbContext.BulkInsertAsync2(items, new SubsetBulkArgs());
        }

        private async Task MarkTeksAsPublishedAsync(long[] used)
        {
            var zap = _workflowDbContext.TemporaryExposureKeys
                .AsNoTracking()
                .Where(x => x.PublishingState == PublishingState.Unpublished && used.Contains(x.Id)).ToList();

            _commitIndex += used.Length;
            _logger.LogInformation("Marking TEKs as Published - Count:{Count}, Running total:{RunningTotal}.", zap.Count, _commitIndex);

            foreach (var i in zap)
            {
                i.PublishingState = PublishingState.Published;
            }

            await _workflowDbContext.BulkUpdateAsync2(zap, new SubsetBulkArgs());
        }

        private DiagnosisKeyInputEntity[] ReadDkPage()
        {
            var q = _dkSourceDbContext.DiagnosisKeysInput
                .AsNoTracking()
                .OrderBy(x => x.TekId)
                .Skip(_commitIndex)
                .Take(10000)
                .ToArray();

            return q.ToArray();
        }
    }
}
