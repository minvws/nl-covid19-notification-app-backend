// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

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
                VaccinationCoverage = MapVaccinationCoverage(input.VaccineCoveragePerAgeGroup, input.BoosterCoverage)
            };
        }

        private static PositiveTestResults MapPositiveTestResults(TestedOverallInput overallInput, TestedGgdInput ggdInput, int cutOffInDays)
        {
            return new PositiveTestResults
            {
                Values = MapPositiveTestResultsValues(overallInput.Values.Where(x => dateWithinCutOffRange(x.DateUnix, 0, cutOffInDays))),
                InfectedMovingAverage = new MovingAverageValue
                {
                    TimestampEnd = overallInput.LastValue.DateUnix,
                    TimestampStart = (long)TimeConverter.ToDateTime(overallInput.LastValue.DateUnix).AddDays(-7).ToDateUnix(),
                    Value = overallInput.LastValue.InfectedMovingAverageRounded
                },
                InfectedPercentage = ggdInput.LastValue.InfectedPercentage,
                HighlightedValue = MapPositiveTestResultsValues(new TestedOverallInputValue[] { overallInput.LastValue }).Single()
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

            var firstUsableMovingAverageValue = input.Values
                .OrderByDescending(x => x.DateUnix)
                .First(x => x.AdmissionsOnDateOfAdmissionMovingAverageRounded != null);

            return new HospitalAdmissions
            {
                Values = MapHospitalAdmissionsValues(input.Values.Where(
                    x => dateWithinCutOffRange(x.DateUnix, offsetInDays, cutOffInDays))),
                HospitalAdmissionMovingAverage = new MovingAverageValue
                {
                    TimestampEnd = firstUsableMovingAverageValue.DateUnix,
                    TimestampStart = (long)TimeConverter.ToDateTime(firstUsableMovingAverageValue.DateUnix).AddDays(-6).ToDateUnix(),
                    Value = firstUsableMovingAverageValue.AdmissionsOnDateOfAdmissionMovingAverageRounded
                },
                HighlightedValue = MapHospitalAdmissionsValues(new HospitalAdmissionsInputValue[] { input.LastValue }).Single()
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

            var firstUsableMovingAverageValue = input.Values
                .OrderByDescending(x => x.DateUnix)
                .First(x => x.AdmissionsOnDateOfAdmissionMovingAverageRounded != null);

            return new IcuAdmissions
            {
                Values = MapIcuAdmissionsValues(input.Values.Where(
                    x => dateWithinCutOffRange(x.DateUnix, offsetInDays, cutOffInDays))),
                IcuAdmissionMovingAverage = new MovingAverageValue
                {
                    TimestampEnd = firstUsableMovingAverageValue.DateUnix,
                    TimestampStart = (long)TimeConverter.ToDateTime(firstUsableMovingAverageValue.DateUnix).AddDays(-7).ToDateUnix(),
                    Value = firstUsableMovingAverageValue.AdmissionsOnDateOfAdmissionMovingAverageRounded
                },
                HighlightedValue = MapIcuAdmissionsValues(new HospitalAdmissionsInputValue[] { input.LastValue }).Single()
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
            BoosterCoverageInput boosterInput)
        {
            return new VaccinationCoverage
            {
                VaccinationCoverage18Plus = vaccineInput.LastValue.Age18_PlusFullyVaccinated,
                BoosterCoverage18Plus = boosterInput.Values.Where(x => "18+".Equals(x.AgeGroup)).FirstOrDefault().Percentage
            };
        }

        private static readonly Func<long, int, int, bool> dateWithinCutOffRange =
            (dateUnix, startAtDay, cutOffDay) => dateUnix > DateTime.UtcNow.AddDays(-cutOffDay).ToDateUnix()
                && dateUnix < DateTime.UtcNow.AddDays(-startAtDay).ToDateUnix();
    }
}
