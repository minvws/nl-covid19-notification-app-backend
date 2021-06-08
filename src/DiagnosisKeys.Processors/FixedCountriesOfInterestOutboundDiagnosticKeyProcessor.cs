// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public class FixedCountriesOfInterestOutboundDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        private readonly string[] _value;

        public FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(IOutboundFixedCountriesOfInterestSetting settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _value = settings.CountriesOfInterest;
        }

        public DkProcessingItem Execute(DkProcessingItem value)
        {
            if (value.DiagnosisKey.Origin != TekOrigin.Local)
            {
                throw new InvalidOperationException("This is a local processor for local DKs. You wouldn't like it here...");
            }

            value.DiagnosisKey.Efgs.CountriesOfInterest = string.Join(",", _value);
            return value;
        }
    }
}
