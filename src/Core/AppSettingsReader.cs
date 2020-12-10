// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public abstract class AppSettingsReader
    {
        private readonly IConfiguration _Config;
        private readonly string _Prefix;

        protected AppSettingsReader(IConfiguration config, string prefix = null)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(prefix) || prefix != prefix.Trim())
                _Prefix = string.Empty;
            else
                _Prefix = prefix + ":";
        }

        protected T GetConfigValue<T>(string path, T defaultValue = default)
            => _Config.GetValue($"{_Prefix}{path}", defaultValue);
    }
}