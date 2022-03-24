// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader
{
    public class DashboardOutputModel
    {
        public PositiveTestResults PositiveTestResults { get; set; }
        public HospitalAdmissions HospitalAdmissions { get; set; }
        public IcuAdmissions IcuAdmissions { get; set; }
        public VaccinationCoverage VaccinationCoverage { get; set; }
        public CoronaMelderUsers CoronaMelderUsers { get; set; }
    }

    public class PositiveTestResults
    {
        public int SortingValue { get; set; } = 0;
        public List<PositiveTestResultsValue> Values { get; set; }
        public double InfectedPercentage { get; set; }
        public long? InfectedMovingAverage { get; set; }
        public PositiveTestResultsValue HighlightedValue { get; set; }
    }

    public class PositiveTestResultsValue
    {
        public long Timestamp { get; set; }
        public long Value { get; set; }
    }

    public class CoronaMelderUsers
    {
        public int SortingValue { get; set; } = 1;
        public List<CoronaMelderUsersValue> Values { get; set; }
        public CoronaMelderUsersValue HighlightedValue { get; set; }
    }

    public class CoronaMelderUsersValue
    {
        public long Timestamp { get; set; }
        public long Value { get; set; }
    }

    public class HospitalAdmissions
    {
        public int SortingValue { get; set; } = 2;
        public List<HospitalAdmissionsValue> Values { get; set; }
        public long? HospitalAdmissionMovingAverage { get; set; }
        public HospitalAdmissionsValue HighlightedValue { get; set; }
    }

    public class HospitalAdmissionsValue
    {
        public long Timestamp { get; set; }
        public long Value { get; set; }
    }

    public class IcuAdmissions
    {
        public int SortingValue { get; set; } = 3;
        public List<IcuAdmissionsValue> Values { get; set; }
        public long? IcuAdmissionMovingAverage { get; set; }
        public IcuAdmissionsValue HighlightedValue { get; set; }
    }

    public class IcuAdmissionsValue
    {
        public long Timestamp { get; set; }
        public long Value { get; set; }
    }

    public class VaccinationCoverage
    {
        public int SortingValue { get; set; } = 4;

        public double VaccinationCoverage18Plus { get; set; }
        public double BoosterCoverage18Plus { get; set; }
        public BoosterCoverage BoosterCoverage { get; set; }
    }

    public class BoosterCoverage
    {
        public List<BoosterCoverageValue> Values { get; set; }
        public BoosterCoverageValue HighlightedValue { get; set; }
    }

    public class BoosterCoverageValue
    {
        public long Timestamp { get; set; }
        public double Value { get; set; }
    }
}
