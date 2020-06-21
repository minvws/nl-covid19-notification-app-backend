namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public interface IReceiverConfig
    {
        string Manifest { get; }
        string AppConfig { get; }
        string ExposureKeySet { get; }
        string RiskCalculationParameters { get; }
        string ResourceBundle { get; }
    }
}