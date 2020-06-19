// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Models
{
    public class RevokeBatchInput
    {
        [Required] public string BatchId { get; set; }
        
        public DateTime? RevokeDateTime { get; set; }
    }
}