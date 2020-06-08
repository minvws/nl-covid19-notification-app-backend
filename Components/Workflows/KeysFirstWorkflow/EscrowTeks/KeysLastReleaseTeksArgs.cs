namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks
{
    /// <summary>
    /// Diagnosis key
    /// </summary>
    public class KeysLastReleaseTeksArgs
    {
        public string Token { get; set; }
        public TemporaryExposureKeyArgs[] Items { get; set; }
    }
}