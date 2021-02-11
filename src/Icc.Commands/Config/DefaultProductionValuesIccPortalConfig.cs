using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public class DefaultProductionValuesIccPortalConfig : IIccPortalConfig
    {
        public string FrontendBaseUrl => throw new MissingConfigurationValueException(nameof(FrontendBaseUrl));
        public string JwtSecret => throw new MissingConfigurationValueException(nameof(JwtSecret));
        public double ClaimLifetimeHours => 3.0;
        public bool StrictRolePolicyEnabled => true;
    }
}