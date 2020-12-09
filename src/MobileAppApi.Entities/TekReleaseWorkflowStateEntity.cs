// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities
{
    [Table("TekReleaseWorkflowState")]
    public class TekReleaseWorkflowStateEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// TODO Could be a date without time.
        /// </summary>
        public DateTime Created { get; set; }
        public DateTime ValidUntil { get; set; }

        [MinLength(6), MaxLength(6)]
        public string? LabConfirmationId { get; set; }
        
        [MinLength(32), MaxLength(32)]
        public byte[] ConfirmationKey { get; set; }

        [MinLength(32), MaxLength(32)]
        public byte[] BucketId { get; set; }

        //public bool CanPublish { get; set; } // == Has Keys + AuthorisedByCaregiver != null

        /// <summary>
        /// From Icc 
        /// </summary>
        public DateTime? AuthorisedByCaregiver { get; set; }

        /// <summary>
        /// From Icc 
        /// </summary>
        public DateTime? DateOfSymptomsOnset { get; set; }

        /// <summary>
        /// Rotating auth token for Icc Portal refresh to see KeysLastUploaded time.
        /// </summary>
        public string? PollToken { get; set; }

        public virtual ICollection<TekEntity> Teks { get; set; } = new List<TekEntity>();
    }
}
