// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding
{
    public enum SymptomsOnsetDatePrecision
    {
        Unknown,
        Range,

        /// <summary>
        /// Not encoded => DSOS
        /// </summary>
        Exact
    }
}
