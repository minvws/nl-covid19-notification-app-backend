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

        protected virtual string Prefix { get; }

        protected AppSettingsReader(IConfiguration config, string? prefix = null)
        {
            _Config = config;

            if (string.IsNullOrWhiteSpace(prefix) || prefix != prefix.Trim())
                Prefix = string.Empty;
            else
                Prefix = prefix + ":";
        }

        protected int GetValueInt32(string path, int defaultValue = int.MinValue)
        {
            var found = _Config[$"{Prefix}{path}"];
            return string.IsNullOrWhiteSpace(found) ? defaultValue : Convert.ToInt32(found);
        }
        protected double GetValueDouble(string path, double defaultValue = 0)
        {
            var found = _Config[$"{Prefix}{path}"];
            return string.IsNullOrWhiteSpace(found) ? defaultValue : Convert.ToDouble(found);
        }

        protected string GetValue(string path, string defaultValue = "Unspecified default")
        {
            var found = _Config[$"{Prefix}{path}"];
            return string.IsNullOrWhiteSpace(found) ? defaultValue : found;
        }

        protected bool GetValueBool(string path, bool defaultValue = false)
        {
            var found = _Config[$"{Prefix}{path}"];
            return string.IsNullOrWhiteSpace(found) ? defaultValue : Convert.ToBoolean(found);
        }
    }
}