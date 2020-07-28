// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TekEntity
    {
        public long Id { get; set; }
        public TekReleaseWorkflowStateEntity Owner { get; set; }
        public byte[] KeyData { get; set; }
        public int RollingStartNumber { get; set; }
        public int RollingPeriod { get; set; }
        public string Region { get; set; }

        public PublishingState PublishingState { get; set; }
        //Written to DB
        public DateTime? Created { get; set; }
        public DateTime? PublishAfter { get; set; }
    }
}