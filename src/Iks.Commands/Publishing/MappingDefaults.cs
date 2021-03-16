// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities;
using EfgsReportType = Iks.Protobuf.EfgsReportType;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing
{
    public static class MappingDefaults
    {

        public static EfgsReportType ToEfgsReportType(this ReportType value)
        {
            return value switch
            {
                ReportType.ConfirmedClinicalDiagnosis => EfgsReportType.ConfirmedClinicalDiagnosis,
                ReportType.ConfirmedTest => EfgsReportType.ConfirmedTest,
                ReportType.Recursive => EfgsReportType.Recursive,
                ReportType.Revoked => EfgsReportType.Revoked,
                ReportType.SelfReport => EfgsReportType.SelfReport,
                _ => EfgsReportType.Unknown
            };
        }

        public static int ToEfgsInt32(this TransmissionRiskLevel value) => (int) value;


        /// <summary>
        /// This is one-to-one mapping. DK Processors with full rules to follow.
        /// </summary>
        /// <param name="value"></param>
        public static InteropKeyFormatterArgs ToInteropKeyFormatterArgs(this IksCreateJobInputEntity value)
            => new InteropKeyFormatterArgs
            {
                Value = value.DailyKey,
                TransmissionRiskLevel = value.TransmissionRiskLevel.ToEfgsInt32(), //TODO or mapper?
                ReportType = value.ReportType.ToEfgsReportType(),
                CountriesOfInterest = value.CountriesOfInterest.Split(","),
                DaysSinceSymtpomsOnset = value.DaysSinceSymptomsOnset
            };
    }
}
