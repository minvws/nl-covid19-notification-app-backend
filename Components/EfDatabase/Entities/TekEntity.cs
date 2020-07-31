// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TekEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public TekReleaseWorkflowStateEntity Owner { get; set; }
        
        [MinLength(32), MaxLength(32)]
        public byte[] KeyData { get; set; } = new byte[32];
        public int RollingStartNumber { get; set; }
        public int RollingPeriod { get; set; }

        [MinLength(2), MaxLength(2)]
        public string Region { get; set; }

        public PublishingState PublishingState { get; set; }
       
        public DateTime PublishAfter { get; set; }
    }
}