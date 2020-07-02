namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public interface IDataApiUrls
    {
        string Username { get; }
        string Password { get; }
        string Manifest { get; }
        string AppConfig { get; }
        string ExposureKeySet { get; }
        string RiskCalculationParameters { get; }
        string ResourceBundle { get; }
    }
}