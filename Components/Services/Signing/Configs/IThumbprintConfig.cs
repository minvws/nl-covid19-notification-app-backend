namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs
{
    public interface IThumbprintConfig
    {
        string Thumbprint { get; }
        bool RootTrusted { get; }
    }
}