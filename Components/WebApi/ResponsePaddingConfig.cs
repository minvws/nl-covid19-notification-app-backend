// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi
{
    public class ResponsePaddingConfig : AppSettingsReader, IResponsePaddingConfig
    {
        public ResponsePaddingConfig(IConfiguration config, string prefix = "ExposureKeySets") : base(config, prefix) { }
        public int MinimumLengthInBytes => GetConfigValue("Workflow:ResponsePadding:ByteCount:Min", 200);
        public int MaximumLengthInBytes => GetConfigValue("Workflow:ResponsePadding:ByteCount:Max", 300);
    }
}