// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    /// <summary>
    /// TODO replace advice with dictionary/array
    /// </summary>
    public class LocalizableTextArgs
    {
        /// <summary>
        /// E.g. en, nl, nl-NL, nl-BE, fr-BE?, de-DE, etc.
        /// See https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.name?view=netcore-3.1
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Base64
        /// Then Moustache...
        ///// </summary>
        public string IsolationAdviceShort { get; set; }

        /// <summary>
        /// Base64
        /// Then Moustache...
        ///// </summary>
        public string IsolationAdviceLong { get; set; }
    }
}