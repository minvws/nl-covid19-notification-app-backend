// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader
{
    public class DashboardInputModel
    {
        [JsonPropertyName("tested_overall")]
        public TestedOverallInput TestedOverallInput { get; set; }

        [JsonPropertyName("tested_ggd")]
        public TestedGgdInput TestedGgdInput { get; set; }

        [JsonPropertyName("hospital_nice")]
        public HospitalAdmissionsInput GeneralHospitalAdmissions { get; set; }

        [JsonPropertyName("intensive_care_nice")]
        public HospitalAdmissionsInput IntensiveCareAdmissions { get; set; }

        [JsonPropertyName("vaccine_coverage_per_age_group_estimated")]
        public VaccineCoveragePerAgeGroupInput VaccineCoveragePerAgeGroup { get; set; }

        [JsonPropertyName("booster_coverage")]
        public BoosterCoverageInput BoosterCoverage { get; set; }
    }

    public class HospitalAdmissionsInput
    {
        [JsonPropertyName("values")]
        public List<HospitalAdmissionsInputValue> Values { get; set; }

        [JsonPropertyName("last_value")]
        public HospitalAdmissionsInputValue LastValue { get; set; }
    }

    public class HospitalAdmissionsInputValue
    {
        [JsonPropertyName("date_unix")]
        public long DateUnix { get; set; }

        [JsonPropertyName("admissions_on_date_of_admission_moving_average_rounded")]
        public long? AdmissionsOnDateOfAdmissionMovingAverageRounded { get; set; }

        [JsonPropertyName("admissions_on_date_of_reporting")]
        public long AdmissionsOnDateOfReporting { get; set; }
    }

    public class TestedGgdInput
    {
        [JsonPropertyName("values")]
        public List<TestedGgdInputValue> Values { get; set; }

        [JsonPropertyName("last_value")]
        public TestedGgdInputValue LastValue { get; set; }
    }

    public class TestedGgdInputValue
    {
        [JsonPropertyName("date_unix")]
        public long DateUnix { get; set; }

        [JsonPropertyName("infected_percentage")]
        public double InfectedPercentage { get; set; }
    }

    public class TestedOverallInput
    {
        [JsonPropertyName("values")]
        public List<TestedOverallInputValue> Values { get; set; }

        [JsonPropertyName("last_value")]
        public TestedOverallInputValue LastValue { get; set; }
    }

    public class TestedOverallInputValue
    {
        [JsonPropertyName("date_unix")]
        public long DateUnix { get; set; }

        [JsonPropertyName("infected_moving_average_rounded")]
        public long? InfectedMovingAverageRounded { get; set; }

        [JsonPropertyName("infected")]
        public long Infected { get; set; }
    }

    public class VaccineCoveragePerAgeGroupInput
    {
        [JsonPropertyName("values")]
        public List<VaccineCoveragePerAgeGroupInputValue> Values { get; set; }

        [JsonPropertyName("last_value")]
        public VaccineCoveragePerAgeGroupInputValue LastValue { get; set; }
    }

    public class VaccineCoveragePerAgeGroupInputValue
    {
        [JsonPropertyName("date_unix")]
        public long DateUnix { get; set; }

        [JsonPropertyName("age_18_plus_fully_vaccinated")]
        public double Age18_PlusFullyVaccinated { get; set; }
    }

    public class BoosterCoverageInput
    {
        [JsonPropertyName("values")]
        public List<BoosterCoverageInputValue> Values { get; set; }
    }

    public class BoosterCoverageInputValue
    {
        [JsonPropertyName("age_group")]
        public string AgeGroup { get; set; }

        [JsonPropertyName("percentage")]
        public double Percentage { get; set; }

        [JsonPropertyName("date_of_insertion_unix")]
        public long DateOfInsertionUnix { get; set; }

        [JsonPropertyName("date_unix")]
        public long DateUnix { get; set; }
    }
}
