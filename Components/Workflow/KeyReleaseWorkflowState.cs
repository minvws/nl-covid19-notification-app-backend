// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class KeyReleaseWorkflowState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime Created { get; set; }
        
        /// <summary>
        /// TODO Add uq
        /// </summary>
        public string? LabConfirmationId { get; set; }

        /// <summary>
        /// For Lab Verify Process
        /// </summary>
        public string? PollToken { get; set; }
        
        public string? ConfirmationKey { get; set; }

        public string? BucketId { get; set; }

        public bool Authorised { get; set; }

        /// <summary>
        /// Explain to me what this is? Why different to Authorised?
        /// </summary>
        public bool AuthorisedByCaregiver { get; set; }

        /// <summary>
        /// Epoch time in seconds
        /// </summary>
        public DateTime ValidUntil { get; set; }
        
        /// <summary>
        /// Date of Symptoms Onset from ICC Portal
        /// </summary>
        public DateTime DateOfSymptomsOnset { get; set; }

        public ICollection<TemporaryExposureKeyEntity> Keys { get; set; } = new List<TemporaryExposureKeyEntity>();
    }
}
