namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class TekWriteArgs
    {
        public TekReleaseWorkflowStateEntity WorkflowStateEntityEntity { get; set; }
        public Tek[] NewItems { get; set; }
    }
}