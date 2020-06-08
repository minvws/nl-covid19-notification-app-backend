// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.KeysFirstWorkflow
{
    public class KeysFirstRandomAuthorisationArgs
    {
        public int Seed { get; set; }

        /// <summary>
        /// Percentage chance of authorisation: 1-100
        /// </summary>
        public int pAuthorize { get; set; }
    }
}
