// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AgProtocolSettings;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Components
{
    /// <summary>
    /// TODO read settings
    /// </summary>
    public class HardCodedAgConfig : IAgConfig
    {
        public int WorkflowSecretLifetimeDays => throw new NotImplementedException();
        public int ExposureKeySetLifetimeDays => throw new NotImplementedException();
        public int ExposureKeySetCapacity => 10; //Cos we want multiple exposure key sets.
        public double ManifestLifeTimeHours { get; set; }
    }
}