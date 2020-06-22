// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration
{
    public class EnvironmentSettingsReader
    {
        protected virtual string Prefix { get; }

        protected EnvironmentSettingsReader(string? prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix != prefix.Trim())
                Prefix = string.Empty;
            else
                Prefix = prefix + ":";
        }

        protected double GetValueDouble(string path, double defaultValue = 0)
        {
            var found = Environment.GetEnvironmentVariable($"{Prefix}{path}");
            return string.IsNullOrWhiteSpace(found) ? defaultValue : Convert.ToDouble(found);
        }
    }
}
