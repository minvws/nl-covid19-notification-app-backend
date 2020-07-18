// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration
{
    public abstract class AppSettingsReader
    {
        private readonly IConfiguration _Config;
        protected IConfiguration Config => _Config;
        protected virtual string Prefix { get; }

        protected AppSettingsReader(IConfiguration config, string? prefix = null)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(prefix) || prefix != prefix.Trim())
                Prefix = string.Empty;
            else
                Prefix = prefix + ":";
        }

        protected int GetValueInt32(string path, int defaultValue = default(int))
            => Config.GetValue($"{Prefix}{path}", defaultValue);

        protected double GetValueDouble(string path, double defaultValue = default(double))
            => Config.GetValue($"{Prefix}{path}", defaultValue);

        protected string GetValue(string path, string defaultValue = "Unspecified default!")
            => Config.GetValue($"{Prefix}{path}", defaultValue);

        protected bool GetValueBool(string path, bool defaultValue = false)
            => Config.GetValue($"{Prefix}{path}", defaultValue);
    }
}