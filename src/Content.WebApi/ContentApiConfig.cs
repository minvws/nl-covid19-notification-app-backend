// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.WebApi
{
    public class ContentApiConfig : AppSettingsReader
    {
        public ContentApiConfig(IConfiguration config, string prefix = "ContentApi") : base(config, prefix)
        {
        }

        /// <summary>
        /// TODO investigate use of this value and its defaults.
        /// </summary>
        public string Url => GetConfigValue(nameof(Url), "https://localhost:5001");
    }
}