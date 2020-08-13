using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class MarkWorkFlowTeksAsUsed : IMarkWorkFlowTeksAsUsed
    {
        private readonly Func<WorkflowDbContext> _WorkflowDbContextFactory;
        private readonly IEksConfig _EksConfig;
        private readonly Func<PublishingJobDbContext> _PublishingDbContextFac;
        private readonly ILogger<MarkWorkFlowTeksAsUsed> _Logger;
        private int _Index;

        public MarkWorkFlowTeksAsUsed(Func<WorkflowDbContext> workflowDbContextFactory, IEksConfig eksConfig, Func<PublishingJobDbContext> publishingDbContextFac, ILogger<MarkWorkFlowTeksAsUsed> logger)
        {
            _WorkflowDbContextFactory = workflowDbContextFactory ?? throw new ArgumentNullException(nameof(workflowDbContextFactory));
            _EksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _PublishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MarkWorkFlowTeksAsUsedResult> ExecuteAsync()
        {
            var used = ReadPage();
            while (used.Length > 0)
            {
                await Zap(used);
                used = ReadPage();
            }
            return new MarkWorkFlowTeksAsUsedResult { Marked = _Index };
        }

        private async Task Zap(long[] used)
        {
            await using var wfDb = _WorkflowDbContextFactory();

            var zap = wfDb.TemporaryExposureKeys
                .Where(x => used.Contains(x.Id))
                .ToList();

            _Index += used.Length;
            _Logger.LogInformation($"Marking as Published - Count:{zap.Count}, Running total:{_Index}.");

            if (zap.Count == 0)
                return;

            foreach (var i in zap)
            {
                i.PublishingState = PublishingState.Published;
            }

            var bargs = new SubsetBulkArgs 
            {
                PropertiesToInclude = new [] { nameof(TekEntity.PublishingState) }
            };
                
            await wfDb.BulkUpdateAsync2(zap, bargs); //TX
        }

        private long[] ReadPage()
        {
            //No tx cos nothing else is touching this context.
            //New context each time
            return _PublishingDbContextFac().Set<EksCreateJobInputEntity>()
                .Where(x => x.TekId != null && (x.Used || x.TransmissionRiskLevel == TransmissionRiskLevel.None))
                .OrderBy(x => x.TekId)
                .Skip(_Index)
                .Take(_EksConfig.PageSize) //TODO May need separate setting to tune.
                .Select(x => x.TekId.Value)
                .ToArray();
        }
    }
}