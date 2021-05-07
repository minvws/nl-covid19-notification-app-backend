// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class TemporaryExposureKeyArgs
    {
        public byte[] KeyData { get; set; }
        public int RollingStartNumber { get; set; }
        public int RollingPeriod { get; set; }
        [Obsolete("Not used for GAEN V2")]
        public TransmissionRiskLevel TransmissionRiskLevel { get; set; }
        public int DaysSinceOnsetSymptoms { get; set; }
        public InfectiousPeriodType Symptomatic { get; set; }
        public int ReportType { get; set; }
    }
}