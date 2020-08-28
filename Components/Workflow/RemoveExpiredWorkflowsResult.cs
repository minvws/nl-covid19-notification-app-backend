namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Expiry
{
    public class RemoveExpiredWorkflowsResult
    {
        public WorkflowStats Before { get; } = new WorkflowStats();
        public bool DeletionsOn { get; set; }
        public int UnauthorisedKilled { get; set; }
        public int AuthorisedAndFullyPublishedKilled { get; set; }
        public WorkflowStats After { get; } = new WorkflowStats();
    }
}