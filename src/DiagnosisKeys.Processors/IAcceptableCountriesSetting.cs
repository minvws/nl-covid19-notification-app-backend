// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    /// <summary>
    /// Very granular if/until we design string-based config for DK processors.
    /// Add this interface to a section/group, then pass that AppSettingReader
    /// </summary>
    public interface IAcceptableCountriesSetting
    {
        string[] AcceptableCountries { get; }
    }
}