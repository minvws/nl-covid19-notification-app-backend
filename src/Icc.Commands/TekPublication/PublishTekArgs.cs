// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication
{
    /// <summary>
    /// Temporary Exposure Key publication arguments
    /// </summary>
    public class PublishTekArgs
    {
        /// <summary>
        /// Is a string composed of six characters followed by one checksum character.
        /// The checksum is calculated according to the Luhn mod N algorithm (which is a modification to the original Luhn algorithm as described in ISO/IEC 8712 Annex B).
        /// </summary>
        public string GGDKey { get; set; }
        /// <summary>
        /// A boolean value which is set to true if the subject has symptoms (symptomatic), false otherwise (asymptomatic) 
        /// </summary>
        public bool Symptomatic { get; set; }
        /// <summary>
        /// The given StartOfInfectiousPeriod or Date of Test entered by the GGD user
        /// </summary>
        public DateTime? SelectedDate { get; set; }
    }
}
