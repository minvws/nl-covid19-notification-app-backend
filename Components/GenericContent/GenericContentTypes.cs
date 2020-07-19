namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ContentLoading
{
    public static class GenericContentTypes
    {
        public const string AppConfig = nameof(AppConfig);
        public const string RiskCalculationParameters = nameof(RiskCalculationParameters); //TODO API version?

        public static bool IsValid(string value) => value == AppConfig || value == RiskCalculationParameters;
    }
}