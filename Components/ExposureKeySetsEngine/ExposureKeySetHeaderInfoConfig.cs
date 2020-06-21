// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs
{
    public class ExposureKeySetHeaderInfoConfig : AppSettingsReader, IExposureKeySetHeaderInfoConfig
    {
        public ExposureKeySetHeaderInfoConfig(IConfiguration config, string? prefix = null) : base(config, prefix) { }

        public string AppBundleId => GetValue("ExposureKeySets:AppleId", "nl.rijksoverheid.samensterk");
        public string VerificationKeyId => GetValue("ExposureKeySets:Signing:VerificationKeyId", "ServerNL");
        public string VerificationKeyVersion => GetValue("ExposureKeySets:Signing:VerificationKeyVersion", "v1");
    }
}