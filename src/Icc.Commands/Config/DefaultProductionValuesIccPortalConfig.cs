using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public class DefaultProductionValuesIccPortalConfig : IIccPortalConfig
    {
        //Mandatory in config file - do not use in AppSettingsReader implementation
        public string FrontendBaseUrl => throw new NotImplementedException();

        //Mandatory in config file - do not use in AppSettingsReader implementation
        public string BackendBaseUrl => throw new NotImplementedException();

        //Mandatory in config file - do not use in AppSettingsReader implementation
        public string JwtSecret => throw new NotImplementedException();
        public double ClaimLifetimeHours => 3.0;
        public bool StrictRolePolicyEnabled => true;
    }
}