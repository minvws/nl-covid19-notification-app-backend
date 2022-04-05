// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Downloader
{
    public static class DashboardDataMapper
    {
        public static DashboardOutputModel Map(DashboardInputModel input)
        {
            return new DashboardOutputModel
            {
                PositiveTestResults = MapPositiveTestResults(input.TestedOverallInput, input.TestedGgdInput),
                CoronaMelderUsers = MapCoronaMelderUsers(input.CoronaMelderUsersInput),
                HospitalAdmissions = MapHospitalAdmissions(input.GeneralHospitalAdmissions),
                IcuAdmissions = MapIcuAdmissions(input.IntensiveCareAdmissions),
                VaccinationCoverage = MapVaccinationCoverage(input.VaccineCoveragePerAgeGroup, input.BoosterCoverage)
            };
        }

        private static PositiveTestResults MapPositiveTestResults(TestedOverallInput overallInput, TestedGgdInput ggdInput)
        {
            return new PositiveTestResults
            {
                Values = overallInput.Values.Select(x => MapPositiveTestResultsValue(x)).ToList(),
                InfectedMovingAverage = new MovingAverageValue
                {
                    TimestampEnd = overallInput.LastValue.DateUnix,
                    TimestampStart = (long)TimeConverter.ToDateTime(overallInput.LastValue.DateUnix).AddDays(-7).ToDateUnix(),
                    Value = overallInput.LastValue.InfectedMovingAverageRounded
                },
                InfectedPercentage = ggdInput.LastValue.InfectedPercentage,
                HighlightedValue = MapPositiveTestResultsValue(overallInput.LastValue)
            };
        }

        private static DashboardOutputModelItemValue MapPositiveTestResultsValue(TestedOverallInputValue input)
        {
            return new DashboardOutputModelItemValue
            {
                Timestamp = input.DateUnix,
                Value = input.Infected
            };
        }

        private static CoronaMelderUsers MapCoronaMelderUsers(CoronaMelderUsersInput input)
        {
            var mappedValues = input.Values.Select(x => MapCoronaMelderUsersValue(x)).ToList();

            return new CoronaMelderUsers
            {
                Values = mappedValues,
                HighlightedValue = mappedValues.OrderByDescending(x => x.Timestamp).First()
            };
        }

        private static CoronaMelderUsersValue MapCoronaMelderUsersValue(CoronaMelderUsersInputValue input)
        {
            return new CoronaMelderUsersValue
            {
                Value = input.AverageDailyUsers,
                Timestamp = new DateTimeOffset(input.LastDate).ToUnixTimeSeconds()
            };
        }

        private static HospitalAdmissions MapHospitalAdmissions(HospitalAdmissionsInput input)
        {
            var firstUsableMovingAverageValue = input.Values
                .OrderByDescending(x => x.DateUnix)
                .First(x => x.AdmissionsOnDateOfAdmissionMovingAverageRounded != null);

            return new HospitalAdmissions
            {
                Values = input.Values.Select(x => MapHospitalAdmissionsValue(x)).ToList(),
                HospitalAdmissionMovingAverage = new MovingAverageValue
                {
                    TimestampEnd = firstUsableMovingAverageValue.DateUnix,
                    TimestampStart = (long)TimeConverter.ToDateTime(firstUsableMovingAverageValue.DateUnix).AddDays(-6).ToDateUnix(),
                    Value = firstUsableMovingAverageValue.AdmissionsOnDateOfAdmissionMovingAverageRounded
                },
                HighlightedValue = MapHospitalAdmissionsValue(input.LastValue)
            };
        }

        private static IcuAdmissions MapIcuAdmissions(HospitalAdmissionsInput input)
        {
            var firstUsableMovingAverageValue = input.Values
                .OrderByDescending(x => x.DateUnix)
                .First(x => x.AdmissionsOnDateOfAdmissionMovingAverageRounded != null);

            return new IcuAdmissions
            {
                Values = input.Values.Select(x => MapHospitalAdmissionsValue(x)).ToList(),
                IcuAdmissionMovingAverage = new MovingAverageValue
                {
                    TimestampEnd = firstUsableMovingAverageValue.DateUnix,
                    TimestampStart = (long)TimeConverter.ToDateTime(firstUsableMovingAverageValue.DateUnix).AddDays(-7).ToDateUnix(),
                    Value = firstUsableMovingAverageValue.AdmissionsOnDateOfAdmissionMovingAverageRounded
                },
                HighlightedValue = MapHospitalAdmissionsValue(input.LastValue)
            };
        }

        private static DashboardOutputModelItemValue MapHospitalAdmissionsValue(HospitalAdmissionsInputValue input)
        {
            return new DashboardOutputModelItemValue
            {
                Timestamp = input.DateUnix,
                Value = input.AdmissionsOnDateOfReporting
            };
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
    }
}
