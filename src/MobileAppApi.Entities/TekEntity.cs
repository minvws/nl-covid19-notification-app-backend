// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities
{
    public class TekEntity
    {
        public long Id { get; set; }
        public TekReleaseWorkflowStateEntity Owner { get; set; }
        
        [MinLength(UniversalConstants.DailyKeyDataByteCount), MaxLength(UniversalConstants.DailyKeyDataByteCount)]
        public byte[] KeyData { get; set; } = new byte[UniversalConstants.DailyKeyDataByteCount];
        public int RollingStartNumber { get; set; }
        public int RollingPeriod { get; set; }

        public PublishingState PublishingState { get; set; }
       
        public DateTime PublishAfter { get; set; }
    }
}