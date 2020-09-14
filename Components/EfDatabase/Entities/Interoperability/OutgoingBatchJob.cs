// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities
{
    [Table(TableNames.OutgoingBatchJobs)]
    public class OutgoingBatchJob
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime StartedOn { get; set; }

        public DateTime CompletedAt { get; set; }

        public OutgoingBatchJobStatus Status { get; set; }

        public int RetryCount { get; set; }

        public int TotalKeys { get; set; }
    }
}