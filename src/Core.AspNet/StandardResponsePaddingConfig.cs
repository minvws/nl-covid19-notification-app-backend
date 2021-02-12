// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public class StandardResponsePaddingConfig : AppSettingsReader, IResponsePaddingConfig
    {
        private static readonly ProductionDefaultValuesResponsePaddingConfig _ProductionDefaultValues = new ProductionDefaultValuesResponsePaddingConfig();

        public StandardResponsePaddingConfig(IConfiguration config, string prefix = "Workflow:ResponsePadding") : base(config, prefix) { }
        public int ByteCountMinimum => GetConfigValue("ByteCount:Min", _ProductionDefaultValues.ByteCountMinimum);
        public int ByteCountMaximum => GetConfigValue("ByteCount:Max", _ProductionDefaultValues.ByteCountMaximum);
    }
}