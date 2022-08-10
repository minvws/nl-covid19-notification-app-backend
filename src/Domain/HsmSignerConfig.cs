// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

public class HsmSignerConfig : AppSettingsReader, IHsmSignerConfig
{
    public HsmSignerConfig(IConfiguration config, string prefix = "HsmSigner") : base(config, prefix)
    {
    }

    public string BaseAddress => GetConfigValue(nameof(BaseAddress), string.Empty);
    public string NlJwt => GetConfigValue(nameof(NlJwt), string.Empty);
    public string GaenJwt => GetConfigValue(nameof(GaenJwt), string.Empty);
    public string NlPem => GetConfigValue(nameof(NlPem), string.Empty);
    public string GaenPem => GetConfigValue(nameof(GaenPem), string.Empty);
}
