// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader
{
    public class ProductionDefaultValuesDashboardDataConfig : IDashboardDataConfig
    {
        public string DashboardDataDownloadUrl => string.Empty;
        public int CutOffInDays => 35;
        public string DashboardOverviewExternalLink => string.Empty;
        public string PositiveTestResultsExternalLink => string.Empty;
        public string HospitalAdmissionsExternalLink => string.Empty;
        public string IcuAdmissionsExternalLink => string.Empty;
        public string VaccinationCoverageExternalLink => string.Empty;
        public string CoronaMelderUsersExternalLink => string.Empty;
    }
}
