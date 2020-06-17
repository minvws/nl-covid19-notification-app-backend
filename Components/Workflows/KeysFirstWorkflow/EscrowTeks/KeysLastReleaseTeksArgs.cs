namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks
{
    /// <summary>
    /// Diagnosis key
    /// </summary>
    public class KeysLastReleaseTeksArgs
    {
        public string BucketId { get; set; }
        public TemporaryExposureKeyArgs[] Keys { get; set; }
        public byte[]? Padding { get; set; }
    }
}