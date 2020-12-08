//// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
//// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
//// SPDX-License-Identifier: EUPL-1.2

//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound.Publishing;
//using EfgsReportType = Eu.Interop.EfgsReportType;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
//{
//    public static class MappingDefaults
//    {
//        public static EfgsReportTypeInternal ToEfgsReportTypeInternal(this EfgsReportType value)
//        {
//            return value switch
//            {
//                EfgsReportType.ConfirmedClinicalDiagnosis => EfgsReportTypeInternal.ConfirmedClinicalDiagnosis,
//                EfgsReportType.ConfirmedTest => EfgsReportTypeInternal.ConfirmedTest,
//                EfgsReportType.Recursive => EfgsReportTypeInternal.Recursive,
//                EfgsReportType.Revoked => EfgsReportTypeInternal.Revoked,
//                EfgsReportType.SelfReport => EfgsReportTypeInternal.SelfReport,
//                _ => EfgsReportTypeInternal.Unknown
//            };
//        }

//        public static EfgsReportType ToEfgsReportType(this EfgsReportTypeInternal value)
//        {
//            return value switch
//            {
//                EfgsReportTypeInternal.ConfirmedClinicalDiagnosis => EfgsReportType.ConfirmedClinicalDiagnosis,
//                EfgsReportTypeInternal.ConfirmedTest => EfgsReportType.ConfirmedTest,
//                EfgsReportTypeInternal.Recursive => EfgsReportType.Recursive,
//                EfgsReportTypeInternal.Revoked => EfgsReportType.Revoked,
//                EfgsReportTypeInternal.SelfReport => EfgsReportType.SelfReport,
//                _ => EfgsReportType.Unknown
//            };
//        }
//    }
//}