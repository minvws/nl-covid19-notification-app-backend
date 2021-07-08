// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public class NlAcceptableCountryInboundDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        private readonly HashSet<string> _include;

        public NlAcceptableCountryInboundDiagnosticKeyProcessor(IAcceptableCountriesSetting settings)
        {
            _include = settings.AcceptableCountries.ToHashSet();
        }

        //Assumes a cleaning processor has already done Trim/ToUpper
        public DkProcessingItem Execute(DkProcessingItem value)
            => value.Metadata.TryGetValue("Source", out var source) && _include.Contains(source) ? value : null;
    }
}
