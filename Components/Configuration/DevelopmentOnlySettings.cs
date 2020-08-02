// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration
{
    public static class DevelopmentOnlySettings
    { 
        private const string Prefix = "DevelopmentFlags:";
        private const string KillAuthName = Prefix + "KillAuth";
        private const string UseCertificatesFromResourcesName = Prefix + "UseCertificatesFromResources";
        private const string ValidatePostKeysSignatureName = Prefix + "ValidatePostKeysSignature";
        

        public static bool KillAuth(this IConfiguration configuration)
            => configuration.GetValue(KillAuthName, false);

        public static bool UseCertificatesFromResources(this IConfiguration configuration)
            => configuration.GetValue(UseCertificatesFromResourcesName, false);

        public static bool ValidatePostKeysSignature(this IConfiguration configuration)
            => configuration.GetValue(ValidatePostKeysSignatureName, true);
    }
}