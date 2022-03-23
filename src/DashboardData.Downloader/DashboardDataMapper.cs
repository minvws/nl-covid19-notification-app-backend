// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader
{
    public static class DashboardDataMapper
    {
        public static DashboardOutputModel Map(DashboardInputModel input, int cutOffInDays = 35)
        {
            return new DashboardOutputModel
            {
                PositiveTestResults = MapPositiveTestResults(input.TestedOverallInput, input.TestedGgdInput, cutOffInDays),
                HospitalAdmissions = MapHospitalAdmissions(input.GeneralHospitalAdmissions, cutOffInDays),
                IcuAdmissions = MapIcuAdmissions(input.IntensiveCareAdmissions, cutOffInDays),
                VaccinationCoverage = MapVaccinationCoverage(input.VaccineCoveragePerAgeGroup, input.BoosterCoverage, cutOffInDays)
            };
        }

        private static PositiveTestResults MapPositiveTestResults(TestedOverallInput overallInput, TestedGgdInput ggdInput, int cutOffInDays)
        {
            return new PositiveTestResults
            {
                Values = MapPositiveTestResultsValues(overallInput.Values.Where(x => dateWithinCutOffRange(x.DateUnix, 0, cutOffInDays))),
                InfectedMovingAverage = overallInput.LastValue.InfectedMovingAverageRounded,
                InfectedPercentage = ggdInput.LastValue.InfectedPercentage
            };
        }

        private static List<PositiveTestResultsValue> MapPositiveTestResultsValues(IEnumerable<TestedOverallInputValue> input)
        {
            return input.Select(x => new PositiveTestResultsValue
            {
                Timestamp = x.DateUnix,
                Value = x.Infected
            }).ToList();
        }

        private static HospitalAdmissions MapHospitalAdmissions(HospitalAdmissionsInput input, int cutOffInDays)
        {
            // Not all downloaded data is used here; data from t-5 and back is used,
            // as the data for the most recent days is not dependable enough.

            // For the hospital admissions moving average,
            // the value of the most recent date with a non-null value.

            var offsetInDays = 5;

            return new HospitalAdmissions
            {
                Values = MapHospitalAdmissionsValues(input.Values.Where(
                    x => dateWithinCutOffRange(x.DateUnix, offsetInDays, cutOffInDays))),
                HospitalAdmissionMovingAverage = input.Values
                    .OrderByDescending(x => x.DateUnix)
                    .First(x => x.AdmissionsOnDateOfAdmissionMovingAverageRounded != null).AdmissionsOnDateOfAdmissionMovingAverageRounded
            };
        }

        private static List<HospitalAdmissionsValue> MapHospitalAdmissionsValues(IEnumerable<HospitalAdmissionsInputValue> input)
        {
            return input.Select(x => new HospitalAdmissionsValue
            {
                Timestamp = x.DateUnix,
                Value = x.AdmissionsOnDateOfReporting
            }).ToList();
        }

        private static IcuAdmissions MapIcuAdmissions(HospitalAdmissionsInput input, int cutOffInDays)
        {
            // Not all downloaded data is used here; data from t-4 and back is used,
            // as the data for the most recent days is not dependable enough.

            // For the icu admissions moving average,
            // the value of the most recent date with a non-null value.

            var offsetInDays = 4;

            return new IcuAdmissions
            {
                Values = MapIcuAdmissionsValues(input.Values.Where(
                    x => dateWithinCutOffRange(x.DateUnix, offsetInDays, cutOffInDays))),
                IcuAdmissionMovingAverage = input.Values
                    .OrderByDescending(x => x.DateUnix)
                    .First(x => x.AdmissionsOnDateOfAdmissionMovingAverageRounded != null).AdmissionsOnDateOfAdmissionMovingAverageRounded
            };
        }

        private static List<IcuAdmissionsValue> MapIcuAdmissionsValues(IEnumerable<HospitalAdmissionsInputValue> input)
        {
            return input.Select(x => new IcuAdmissionsValue
            {
                Timestamp = x.DateUnix,
                Value = x.AdmissionsOnDateOfReporting
            }).ToList();
        }

        private static VaccinationCoverage MapVaccinationCoverage(
            VaccineCoveragePerAgeGroupInput vaccineInput,
            BoosterCoverageInput boosterInput,
            int cutOffInDays)
        {
            return new VaccinationCoverage
            {
                VaccinationCoverage18Plus = vaccineInput.LastValue.Age18_PlusFullyVaccinated,
                BoosterCoverage18Plus = boosterInput.LastValue.Percentage,
                BoosterCoverage = MapBoosterCoverage(boosterInput, cutOffInDays)
            };
        }

        private static BoosterCoverage MapBoosterCoverage(BoosterCoverageInput boosterInput, int cutOffInDays)
        {
            return new BoosterCoverage
            {
                Values = MapBoosterCoverageValues(boosterInput.Values.Where(x => dateWithinCutOffRange(x.DateUnix, 0, cutOffInDays)))
            };
        }

        private static List<BoosterCoverageValue> MapBoosterCoverageValues(IEnumerable<BoosterCoverageInputValue> input)
        {
            return input.Select(x => new BoosterCoverageValue
            {
                Timestamp = x.DateUnix,
                Value = x.Percentage
            }).ToList();
        }

        private static double ToDateUnix(this DateTime value)
        {
            return value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private static readonly Func<long, int, int, bool> dateWithinCutOffRange =
            (dateUnix, startAtDay, cutOffDay) => dateUnix > DateTime.UtcNow.AddDays(-cutOffDay).ToDateUnix()
                && dateUnix < DateTime.UtcNow.AddDays(-startAtDay).ToDateUnix();
    }
}
