// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities
{
    public class TekReleaseWorkflowStateEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime Created { get; set; }
        public DateTime ValidUntil { get; set; }

        [MinLength(7), MaxLength(7)]
        public string GGDKey { get; set; }

        [MinLength(32), MaxLength(32)]
        public byte[] ConfirmationKey { get; set; }

        [MinLength(32), MaxLength(32)]
        public byte[] BucketId { get; set; }

        /// <summary>
        /// From Icc
        /// </summary>
        public DateTime? AuthorisedByCaregiver { get; set; }

        /// <summary>
        /// This is either the DateOfSymptomsOnset or Date of Test value.
        /// </summary>
        public DateTime? StartDateOfTekInclusion { get; set; }

        /// <summary>
        /// True if Index has symptoms, otherwise false
        /// </summary>
        public InfectiousPeriodType? IsSymptomatic { get; set; }

        public bool IsOriginPortal { get; set; }

        public virtual ICollection<TekEntity> Teks { get; set; } = new List<TekEntity>();
    }
}
