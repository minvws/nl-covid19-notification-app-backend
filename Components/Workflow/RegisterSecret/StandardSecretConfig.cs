// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public class StandardSecretConfig : AppSettingsReader, ISecretConfig
    {
        private static readonly ISecretConfig _Defaults = new DefaultSecretConfig();

        public StandardSecretConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public int ByteCount => GetValueInt32(nameof(ByteCount), _Defaults.ByteCount);
    }
}