// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models;

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

        public AuthorisationArgs(ConfirmLabConfirmationIdModel confirmLabConfirmationIdModel)
        {
            LabConfirmationId = confirmLabConfirmationIdModel.LabConfirmationId?.Replace("-",string.Empty);
            DateOfSymptomsOnset = confirmLabConfirmationIdModel.DateOfSymptomsOnset;
        }
        
        /// <summary>
        /// Identifier for Workflow item - Tan1?
        /// </summary>
        public string? LabConfirmationId { get; set; }
        
        public DateTime DateOfSymptomsOnset { get; set; }
    }
}