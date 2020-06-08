// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow
{
    [Table("KeysLastTeksWorkflowItems")]
    public class KeysLastTeksWorkflowEntity
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string TekContent { get; set; }
        public string Region { get; set; } = DefaultValues.Region;

        /// <summary>
        /// Set to null to free up key space when Authorised.
        /// </summary>
        public string? SecretToken { get; set; }

        /// <summary>
        /// TAN1?
        /// </summary>
        public string? ExternalTestId { get; set; }
        
        /// <summary>
        /// TAN2?
        /// </summary>
        public string? TekWriteAuthorisationToken { get; set; }
        
        /// <summary>
        /// Derivable from Tan/Token state?
        /// </summary>
        public KeysLastWorkflowState State { get; set; }


        /// <summary>
        /// Base time for the job to return the state to Unauthorised.
        /// </summary>
        public DateTime? ReceivingStarted { get; set; }
    }
}