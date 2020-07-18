// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration
{
    public static class DirectConfigurationReader
    {
        public static T GetValue<T>(this IConfiguration configuration, string name, T defaultValue = default(T))
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            return (T)configuration.GetValue(typeof(T), name, defaultValue);
        }
    }
}