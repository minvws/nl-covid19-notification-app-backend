namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public interface INewTeksValidator 
    {
        string[] Validate(Tek[] newKeys, TekReleaseWorkflowStateEntity workflow);
    }
}