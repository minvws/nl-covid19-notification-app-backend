namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public interface ILabConfirmationIdFormatter
    {
        string Format(string value);
        string Parse(string value);
    }
}