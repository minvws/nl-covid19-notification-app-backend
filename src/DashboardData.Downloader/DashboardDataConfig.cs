// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader
{
    public class DashboardDataConfig : AppSettingsReader, IDashboardDataConfig
    {
        private static readonly ProductionDefaultValuesDashboardDataConfig productionDefaultValues = new ProductionDefaultValuesDashboardDataConfig();

        public DashboardDataConfig(IConfiguration configuration, string prefix = "DashboardData")
            : base(configuration, prefix)
        {
        }

        public string DashboardDataDownloadUrl => GetConfigValue(nameof(DashboardDataDownloadUrl), productionDefaultValues.DashboardDataDownloadUrl);
        public int CutOffInDays => GetConfigValue(nameof(CutOffInDays), productionDefaultValues.CutOffInDays);
        public string DashboardOverviewExternalLink => GetConfigValue(nameof(DashboardOverviewExternalLink), productionDefaultValues.DashboardOverviewExternalLink);
        public string PositiveTestResultsExternalLink => GetConfigValue(nameof(PositiveTestResultsExternalLink), productionDefaultValues.PositiveTestResultsExternalLink);
        public string HospitalAdmissionsExternalLink => GetConfigValue(nameof(HospitalAdmissionsExternalLink), productionDefaultValues.HospitalAdmissionsExternalLink);
        public string IcuAdmissionsExternalLink => GetConfigValue(nameof(IcuAdmissionsExternalLink), productionDefaultValues.IcuAdmissionsExternalLink);
        public string VaccinationCoverageExternalLink => GetConfigValue(nameof(VaccinationCoverageExternalLink), productionDefaultValues.VaccinationCoverageExternalLink);
        public string CoronaMelderUsersExternalLink => GetConfigValue(nameof(CoronaMelderUsersExternalLink), productionDefaultValues.CoronaMelderUsersExternalLink);
    }
}
