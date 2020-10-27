using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.Snapshot;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class SnapshotEksInputMk1 : ISnapshotEksInput
    {
        private readonly ILogger<SnapshotEksInputMk1> _Logger;

        private readonly ITransmissionRiskLevelCalculation _TransmissionRiskLevelCalculation;

        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly Func<PublishingJobDbContext> _PublishingDbContextFactory;

        public SnapshotEksInputMk1(ILogger<SnapshotEksInputMk1> logger, ITransmissionRiskLevelCalculation transmissionRiskLevelCalculation, WorkflowDbContext workflowDbContext, Func<PublishingJobDbContext> publishingDbContextFactory)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _TransmissionRiskLevelCalculation = transmissionRiskLevelCalculation ?? throw new ArgumentNullException(nameof(transmissionRiskLevelCalculation));
            _WorkflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _PublishingDbContextFactory = publishingDbContextFactory ?? throw new ArgumentNullException(nameof(publishingDbContextFactory));
        }

        private DateTime _SnapshotStart;

        public async Task<SnapshotEksInputResult> Execute(DateTime snapshotStart)
        {
            _Logger.WriteStart();

            _SnapshotStart = snapshotStart;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const int pagesize = 10000;
            var index = 0;

            using var tx = _WorkflowDbContext.BeginTransaction();
            var page = ReadTeksFromWorkflow(index, pagesize);

            while (page.Count > 0)
            {
                var db = _PublishingDbContextFactory();
                await db.BulkInsertAsync2(page.ToList(), new SubsetBulkArgs());

                index += page.Count;
                page = ReadTeksFromWorkflow(index, pagesize);
            }

            var result = new SnapshotEksInputResult
            {
                SnapshotSeconds = stopwatch.Elapsed.TotalSeconds,
                TekInputCount = index
            };

            _Logger.WriteTeksToPublish(index);

            return result;
        }

        private IList<EksCreateJobInputEntity> ReadTeksFromWorkflow(int index, int pageSize)
        {
            var temp = _WorkflowDbContext.TemporaryExposureKeys
                .Where(x => (x.Owner.AuthorisedByCaregiver != null)
                            && x.Owner.DateOfSymptomsOnset != null
                            && x.PublishingState == PublishingState.Unpublished
                            && x.PublishAfter <= _SnapshotStart
                )
                .Skip(index)
                .Take(pageSize)
                .Select(x => new {
                    x.Id,
                    D = x.KeyData,
                    S = x.RollingStartNumber,
                    //P = x.RollingPeriod, //iOS xxx requires all RP to be 144
                    DateOfSymptomsOnset = x.Owner.DateOfSymptomsOnset.Value
                }).ToList();

            var result = temp
                .Select(x => new EksCreateJobInputEntity
                {
                    TekId = x.Id,
                    RollingStartNumber = x.S,
                    RollingPeriod = 144, //Constant cos iOS xxx requires all RP to be 144
                    KeyData = x.D,
                    TransmissionRiskLevel = _TransmissionRiskLevelCalculation.Calculate(x.S, x.DateOfSymptomsOnset),
                }).ToList();

            return result;
        }
    }
}