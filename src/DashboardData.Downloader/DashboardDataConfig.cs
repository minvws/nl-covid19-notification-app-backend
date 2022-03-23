// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader
{
    public class DashboardDataConfig : AppSettingsReader, IDashboardDataConfig
    {
        public DashboardDataConfig(IConfiguration configuration, string prefix = "DashboardData")
            : base(configuration, prefix)
        {
        }

        public string DashboardDataDownloadUrl => GetConfigValue<string>("DashboardDataDownloadUrl");
        public int CutOffInDays => GetConfigValue<int>("CutOffInDays");
    }
}
