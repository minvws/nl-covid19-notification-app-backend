namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret
{
    public class DefaultKeysLastSecretConfig : IKeysLastSecretConfig
    {
        public int ByteCount => 16; //As per crypto doc.
    }
}