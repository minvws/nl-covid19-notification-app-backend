// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication
{
    /// <summary>
    /// Response object send back by the GGDKey api 
    /// </summary>
    public class PublishTekResponse
    {
        /// <summary>
        /// Response value is true if everything is ok or false otherwise
        /// </summary>
        public bool Valid { get; set; }
    }
}
