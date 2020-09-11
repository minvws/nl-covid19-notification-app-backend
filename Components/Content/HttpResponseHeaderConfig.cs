// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class HttpResponseHeaderConfig : AppSettingsReader, IHttpResponseHeaderConfig
    {
        public HttpResponseHeaderConfig(IConfiguration config, string? prefix = "Content") : base(config, prefix)
        {
        }

        public string ManifestCacheControl => GetConfigValue(nameof(ManifestCacheControl), "s-maxage=30");
        public string ImmutableContentCacheControl => GetConfigValue(nameof(ImmutableContentCacheControl), "immutable");
    }
}