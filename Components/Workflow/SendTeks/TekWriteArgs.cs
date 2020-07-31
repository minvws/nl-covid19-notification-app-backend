using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class TekWriteArgs
    {
        public TekReleaseWorkflowStateEntity WorkflowStateEntityEntity { get; set; }
        public Tek[] NewItems { get; set; }
    }
}