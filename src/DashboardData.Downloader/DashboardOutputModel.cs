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
        public MovingAverageValue InfectedMovingAverage { get; set; }
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
        public MovingAverageValue HospitalAdmissionMovingAverage { get; set; }
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
        public MovingAverageValue IcuAdmissionMovingAverage { get; set; }
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
    }

    public class MovingAverageValue
    {
        public long TimestampStart { get; set; }
        public long TimestampEnd { get; set; }
        public long? Value { get; set; }
    }
}
