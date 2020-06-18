namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    /// <summary>
    /// Diagnosis key
    /// </summary>
    public class ReleaseTeksArgs
    {
        public string BucketId { get; set; }
        public TemporaryExposureKeyArgs[] Keys { get; set; }
        public byte[]? Padding { get; set; }
    }
}