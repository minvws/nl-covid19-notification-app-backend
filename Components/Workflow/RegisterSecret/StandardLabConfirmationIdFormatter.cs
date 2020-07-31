namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public class StandardLabConfirmationIdFormatter : ILabConfirmationIdFormatter
    {
        public string Format(string value) => $"{value.Substring(0, 3)}-{value.Substring(3, 3)}";
        public string Parse(string value) => value.Replace("-", string.Empty).Substring(6);
    }
}