// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound.Publishing
{
    public static class MappingDefaults
    {

        public static Eu.Interop.EfgsReportType ToEfgsReportType(this ReportType value)
        {
            return value switch
            {
                ReportType.ConfirmedClinicalDiagnosis => Eu.Interop.EfgsReportType.ConfirmedClinicalDiagnosis,
                ReportType.ConfirmedTest => Eu.Interop.EfgsReportType.ConfirmedTest,
                ReportType.Recursive => Eu.Interop.EfgsReportType.Recursive,
                ReportType.Revoked => Eu.Interop.EfgsReportType.Revoked,
                ReportType.SelfReport => Eu.Interop.EfgsReportType.SelfReport,
                _ => Eu.Interop.EfgsReportType.Unknown
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

        //TODO for import.
        //public static EfgsReportTypeInternal ToEfgsReportTypeInternal(this EfgsReportType value)
        //{
        //    return value switch
        //    {
        //        EfgsReportType.ConfirmedClinicalDiagnosis => EfgsReportTypeInternal.ConfirmedClinicalDiagnosis,
        //        EfgsReportType.ConfirmedTest => EfgsReportTypeInternal.ConfirmedTest,
        //        EfgsReportType.Recursive => EfgsReportTypeInternal.Recursive,
        //        EfgsReportType.Revoked => EfgsReportTypeInternal.Revoked,
        //        EfgsReportType.SelfReport => EfgsReportTypeInternal.SelfReport,
        //        _ => EfgsReportTypeInternal.Unknown
        //    };
        //}
    }
}
