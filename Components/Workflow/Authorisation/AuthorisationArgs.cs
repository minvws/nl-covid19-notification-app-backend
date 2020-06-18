// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    
    /// <summary>
    /// Lab confirmation...
    /// </summary>
    public class AuthorisationArgs
    {
        /// <summary>
        /// Identifier for Workflow item - Tan1?
        /// </summary>
        public string LabConfirmationID { get; set; }

        public string UploadAuthorisationToken { get; set; }
    }
}