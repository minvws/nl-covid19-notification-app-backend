// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Newtonsoft.Json;

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
        [JsonProperty("ggdKey")] public string GGDKey { get; set; }
        /// <summary>
        /// A boolean value which is set to true if the subject has symptoms (symptomatic), false otherwise (asymptomatic) 
        /// </summary>
        [JsonProperty("subjectHasSymptoms")] public bool SubjectHasSymptoms { get; set; }
        /// <summary>
        /// The given DateOfSymptomsOnset
        /// </summary>
        [JsonProperty("dateOfSymptomsOnset")] public DateTime? DateOfSymptomsOnset { get; set; }
        /// <summary>
        /// The given DateOfTest
        /// </summary>
        [JsonProperty("dateOfTest")] public DateTime? DateOfTest { get; set; }
    }
}
