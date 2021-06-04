// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

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
