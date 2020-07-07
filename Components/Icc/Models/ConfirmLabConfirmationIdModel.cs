// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models
{
    public class ConfirmLabConfirmationIdModel
    {
        [Required, MinLength(6), MaxLength(6)]
        public string? LabConfirmationId { get; set; }

        public DateTime DateOfSymptomsOnset { get; set; } 
    }
}