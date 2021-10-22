// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public class NlTrlFromDecodedDosDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        private readonly ITransmissionRiskLevelCalculationMk2 _trlCalculation;
        private readonly ILogger<NlTrlFromDecodedDosDiagnosticKeyProcessor> _logger;

        public NlTrlFromDecodedDosDiagnosticKeyProcessor(ITransmissionRiskLevelCalculationMk2 trlCalculation, ILogger<NlTrlFromDecodedDosDiagnosticKeyProcessor> logger)
        {
            _trlCalculation = trlCalculation ?? throw new ArgumentNullException(nameof(trlCalculation));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public DkProcessingItem Execute(DkProcessingItem value)
        {
            if (value == null)
            {
                return value;
            }

            if (!value.Metadata.TryGetValue(DosDecodingDiagnosticKeyProcessor.DecodedDsosMetadataKey, out var decodedDos))
            {
                _logger.LogError("[NlTrlFromDecodedDosDiagnosticKeyProcessor] DosDecodingDiagnosticKeyProcessor.DecodedDsosMetadataKey not found");
                return null;
            }

            //decodedDos map to TRL...
            value.DiagnosisKey.Local.TransmissionRiskLevel = GetTrl((DsosDecodeResult)decodedDos);

            return value;
        }

        //Take the highest TRL that can be inferred from the DsosDecodeResult.
        private TransmissionRiskLevel GetTrl(DsosDecodeResult value)
        {
            if (value.SymptomObservation == SymptomObservation.Symptomatic)
            {
                var symptomaticValue = value.AsSymptomatic();

                if (symptomaticValue.SymptomsOnsetDatePrecision == SymptomsOnsetDatePrecision.Exact)
                {
                    return _trlCalculation.Calculate(symptomaticValue.DaysSinceOnsetOfSymptoms);
                }

                if (symptomaticValue.SymptomsOnsetDatePrecision == SymptomsOnsetDatePrecision.Range)
                {
                    return GetTrl(symptomaticValue.DaysSinceLastSymptoms);
                }
            }

            //Default
            return _trlCalculation.Calculate(value.DaysSinceSubmission);
        }

        private TransmissionRiskLevel GetTrl(Range<int> range)
        {
            var result = new List<TransmissionRiskLevel>();
            for (var i = range.Lo; i <= range.Hi; i++)
            {
                result.Add(_trlCalculation.Calculate(i));
            }

            if (result.All(x => x == TransmissionRiskLevel.None))
            {
                return TransmissionRiskLevel.None;
            }

            return result.Max();
        }
    }
}
