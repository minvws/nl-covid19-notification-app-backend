// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using Microsoft.Azure.Documents;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{

    public class WorkflowInputEntity
    {
        /// <summary>
        /// This is the id for this table - taken from the original table - NOT an identity/hilo
        /// </summary>
        public int Id { get; set; }

        //public TemporaryExposureKeyContent[] Keys { get; set; }

        public string Content { get; set; }

        /// <summary>
        /// Set when the exposure key set it written
        /// </summary>
        public bool Used { get; set; }

        public string Region { get; set; }
    }
}