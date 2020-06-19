// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.IccBackend.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    /// <summary>
    /// Lab confirmation...
    /// </summary>
    public class AuthorisationArgs
    {
        public AuthorisationArgs()
        {
        }
        public AuthorisationArgs(RedeemIccModel redeemIccModel, string uploadAuthorisationToken)
        {
            LabConfirmationId = redeemIccModel.LabConfirmationId;
            UploadAuthorisationToken = uploadAuthorisationToken;
        }


        /// <summary>
        /// Identifier for Workflow item - Tan1?
        /// </summary>
        public string? LabConfirmationId { get; set; }

        public string? UploadAuthorisationToken { get; set; }
    }
}