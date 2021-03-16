// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using EfgsReportType = Iks.Protobuf.EfgsReportType;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing
{
    public class InteropKeyFormatterArgs
    {
        public DailyKey Value { get; set; }
        public int DaysSinceSymtpomsOnset { get; set; }
        public string[] CountriesOfInterest { get; set; } = new string[0]; //TODO better values e.g. NL?
        public int TransmissionRiskLevel { get; set; }
        public EfgsReportType ReportType { get; set; } = EfgsReportType.ConfirmedTest;
        public string Origin { get; set; } = "NL";
    }
}
