// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    /// <summary>
    /// TODO not intended for prod.
    /// This case could actually happen - foreign long term visitor in NL.
    /// </summary>
    public class CountryNotThatInterestingDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        public DkProcessingItem? Execute(DkProcessingItem? value)
        {
            if(value == null) return value;

            var list = (string[])value.Metadata["Countries"];

            return (list.Length == 1 && list[0] == "NL") ? null : value;
        }
    }
}