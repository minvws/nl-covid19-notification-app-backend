// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models
{
    [Table("InfectionConfirmationCodes")]
    public class InfectionConfirmationCodeEntity
    {
        /// <summary>
        /// 32-digit Infection Confirmation Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Identification to identify & revoke Batchset of ICC's 
        /// </summary>
        public string? BatchId { get; set; } 
        
        /// <summary>
        /// Timestamp for Code generation
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// User Guid from generator user
        /// </summary>
        public String GeneratedBy { get; set; }

        /// <summary>
        /// Default NULL, if Datetime => Revoked = true
        /// </summary>
        public DateTime? Revoked { get; set; }

        /// <summary>
        /// Default NULL, if Datetime => Used = true
        /// </summary>
        public DateTime? Used { get; set; }

        /// <summary>
        /// User Guid from user that used the Code
        /// </summary>
        public String? UsedBy { get; set; }
        
        public bool IsValid()
        {
            return (Used == null && (Revoked == null || Revoked > DateTime.Now));
        }
    }
}