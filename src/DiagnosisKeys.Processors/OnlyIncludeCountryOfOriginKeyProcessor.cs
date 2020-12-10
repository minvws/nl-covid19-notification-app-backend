// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    /// <summary>
    /// TODO not intended for prod.
    /// This case could actually happen - foreign long term visitor in NL.
    /// </summary>
    public class OnlyIncludeCountryOfOriginKeyProcessor : IDiagnosticKeyProcessor
    {
        private readonly string[] _AcceptedCountries;
        public OnlyIncludeCountryOfOriginKeyProcessor(IAcceptableCountriesSetting settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _AcceptedCountries = settings.AcceptableCountries;
        }

        public DkProcessingItem? Execute(DkProcessingItem? value)
        {
            return _AcceptedCountries.Contains(value.DiagnosisKey.Efgs.CountryOfOrigin) ? value : null;
        }
    }
}