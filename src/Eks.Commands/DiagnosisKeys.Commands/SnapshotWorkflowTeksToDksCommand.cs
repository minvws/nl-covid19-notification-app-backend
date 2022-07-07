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
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands
{
    public class SnapshotWorkflowTeksToDksCommand : BaseCommand
    {
        private readonly ILogger<SnapshotWorkflowTeksToDksCommand> _logger;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ITransmissionRiskLevelCalculationMk2 _transmissionRiskLevelCalculation;
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly DiagnosisKeysDbContext _diagnosisKeysDbContext;
        private readonly IDiagnosticKeyProcessor[] _orderedProcessorList;

        private int _commitIndex;
        private SnapshotWorkflowTeksToDksResult _result;

        public SnapshotWorkflowTeksToDksCommand(
            ILogger<SnapshotWorkflowTeksToDksCommand> logger,
            IUtcDateTimeProvider dateTimeProvider,
            ITransmissionRiskLevelCalculationMk2 transmissionRiskLevelCalculation,
            WorkflowDbContext workflowDbContext,
            DiagnosisKeysDbContext diagnosisKeysDbContext,
            IDiagnosticKeyProcessor[] orderedProcessorList)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _transmissionRiskLevelCalculation = transmissionRiskLevelCalculation ?? throw new ArgumentNullException(nameof(transmissionRiskLevelCalculation));
            _workflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _diagnosisKeysDbContext = diagnosisKeysDbContext ?? throw new ArgumentNullException(nameof(diagnosisKeysDbContext));
            _orderedProcessorList = orderedProcessorList ?? throw new ArgumentNullException(nameof(orderedProcessorList));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException();
            }

            _result = new SnapshotWorkflowTeksToDksResult();
            await ClearJobTablesAsync();
            SnapshotTeks();
            await CommitSnapshotAsync();
            return _result;
        }

        public async Task<ICommandResult> ExecuteAsync<T>(T _)
        {
            return await ExecuteAsync();
        }

        private async Task ClearJobTablesAsync()
        {
            await _diagnosisKeysDbContext.TruncateAsync<DiagnosisKeyInputEntity>();
        }

        private void SnapshotTeks()
        {
            _logger.LogDebug("Snapshot publishable TEKs.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int PageSize = 10000;
            var index = 0;

            var page = ReadFromWorkflow(index, PageSize);

            while (page.Count > 0)
            {
                _diagnosisKeysDbContext.BulkInsertBinaryCopy(page);

                index += page.Count;
                page = ReadFromWorkflow(index, PageSize);
            }

            _result.TekReadCount = index;
        }

        private List<DiagnosisKeyInputEntity> ReadFromWorkflow(int index, int pageSize)
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
                    DailyKey = new DailyKey(x.KeyData, x.RollingStartNumber, x.RollingPeriod),
                    DateOfSymptomsOnset = x.Owner.StartDateOfTekInclusion ?? default,
                    Symptomatic = x.Owner.IsSymptomatic ?? default
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
                WriteToDks(used);
                await MarkTeksAsPublishedAsync(used.Select(x => x.TekId).ToArray());
                used = ReadDkPage();
            }
        }

        private void WriteToDks(DiagnosisKeyInputEntity[] used)
        {
            var q3 = used.Select(x => (DkProcessingItem)new DkProcessingItem
            {
                DiagnosisKey = new DiagnosisKeyEntity
                {
                    DailyKey = x.DailyKey,
                    Local = x.Local,
                    Origin = TekOrigin.Local,
                    Created = DateTime.UtcNow
                },
                Metadata = new Dictionary<string, object>
                {
                    //Depends on complex filtering requiring intermediate results to be communicated between filters.
                }
            }).ToArray();

            var q4 = _orderedProcessorList.Execute(q3);
            var items = q4.Select(x => x.DiagnosisKey).ToList();
            _result.DkCount += items.Count;

            _diagnosisKeysDbContext.BulkInsertBinaryCopy(items);
        }

        private async Task MarkTeksAsPublishedAsync(long[] used)
        {
            var zap = _workflowDbContext.TemporaryExposureKeys
                .AsNoTracking()
                .Where(x => x.PublishingState == PublishingState.Unpublished && used.Contains(x.Id)).ToList();

            _commitIndex += used.Length;
            _logger.LogInformation("Marking TEKs as Published - Count: {Count}, Running total: {RunningTotal}.", zap.Count, _commitIndex);

            var idsToUpdate = string.Join(",", zap.Select(x => x.Id.ToString()).ToArray());

            await _workflowDbContext.BulkUpdateSqlRawAsync(
                tableName: "TemporaryExposureKeys",
                columnName: "PublishingState",
                value: 1,
                ids: idsToUpdate);
        }

        private DiagnosisKeyInputEntity[] ReadDkPage()
        {
            var q = _diagnosisKeysDbContext.DiagnosisKeysInput
                .AsNoTracking()
                .OrderBy(x => x.TekId)
                .Skip(_commitIndex)
                .Take(10000)
                .ToArray();

            return q.ToArray();
        }
    }
}
