namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public class DefaultProductionValuesIccIdentityHubConfig : IIccIdentityHubConfig
    {
        public string BaseUrl => throw new MissingConfigurationValueException(nameof(BaseUrl));

        public string Tenant => throw new MissingConfigurationValueException(nameof(Tenant));

        public string ClientId => throw new MissingConfigurationValueException(nameof(ClientId));

        public string ClientSecret => throw new MissingConfigurationValueException(nameof(ClientSecret));

        public string CallbackPath => throw new MissingConfigurationValueException(nameof(CallbackPath));
    }
}