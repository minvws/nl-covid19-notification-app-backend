// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class HttpResponseHeaderConfig : AppSettingsReader, IHttpResponseHeaderConfig
    {
        private static readonly ProductionDefaultValuesHttpResponseHeaderConfig _ProductionDefaultValues = new ProductionDefaultValuesHttpResponseHeaderConfig();

        public HttpResponseHeaderConfig(IConfiguration config, string prefix = "Content") : base(config, prefix)
        {
        }

        public string ManifestCacheControl => GetConfigValue(nameof(ManifestCacheControl), _ProductionDefaultValues.ManifestCacheControl);
        public string ImmutableContentCacheControl => GetConfigValue(nameof(ImmutableContentCacheControl), _ProductionDefaultValues.ImmutableContentCacheControl);
    }
}