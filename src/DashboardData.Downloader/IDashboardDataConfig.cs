// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader
{
    public interface IDashboardDataConfig
    {
        public string DashboardDataDownloadUrl { get; }
        public int CutOffInDays { get; }
        public string DashboardOverviewExternalLink { get; }
        public string PositiveTestResultsExternalLink { get; }
        public string HospitalAdmissionsExternalLink { get; }
        public string IcuAdmissionsExternalLink { get; }
        public string VaccinationCoverageExternalLink { get; }
        public string CoronaMelderUsersExternalLink { get; }
    }
}
