// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys
{
    public class StandardDecoyKeysConfig : AppSettingsReader, IDecoyKeysConfig
    {
        private static readonly IDecoyKeysConfig _Defaults = new DefaultDecoyKeysConfig();

        public StandardDecoyKeysConfig(IConfiguration config, string? prefix = "Workflow:Decoys") : base(config, prefix)
        {
        }

        public int MinimumDelayInMilliseconds => GetConfigValue(nameof(MinimumDelayInMilliseconds), _Defaults.MinimumDelayInMilliseconds);

        public int MaximumDelayInMilliseconds => GetConfigValue(nameof(MaximumDelayInMilliseconds), _Defaults.MaximumDelayInMilliseconds);
    }
}