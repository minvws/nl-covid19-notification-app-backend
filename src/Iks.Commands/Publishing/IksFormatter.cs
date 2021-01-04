// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Google.Protobuf;
using Iks.Protobuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing
{
    public class IksFormatter
    {
        /// <summary>
        /// Because of the DaysSinceOnsetOfSymptoms is a relative time span, if the system stalls over midnight for any reason, you need to rebuild with this field incremented
        /// </summary>
        /// <returns></returns>
        public byte[] Format(InteropKeyFormatterArgs[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (args.Any(x => x == null)) throw new ArgumentException("At least one element is null.", nameof(args));

            var result = new DiagnosisKeyBatch();
            result.Keys.AddRange(args.Select(Map));
            return result.ToByteArray();
        }

        private DiagnosisKey Map(InteropKeyFormatterArgs arg)
        {
            var result = new DiagnosisKey
            {
                KeyData = ByteString.CopyFrom(arg.Value.KeyData),
                DaysSinceOnsetOfSymptoms =  arg.DaysSinceSymtpomsOnset,  //TODO move to RSN (int) (Math.Floor((_DateTimeProvider.Snapshot - arg.DateOfSyptomsOnset).TotalDays)), 
                RollingPeriod = (uint) arg.Value.RollingPeriod,
                RollingStartIntervalNumber = (uint) arg.Value.RollingStartNumber,
                TransmissionRiskLevel = arg.TransmissionRiskLevel,
                Origin = arg.Origin, 
                ReportType = arg.ReportType,
            };
            result.VisitedCountries.AddRange(arg.CountriesOfInterest); //arg.CountriesOfInterest.Except("NL")?
            return result;
        }
    }
}