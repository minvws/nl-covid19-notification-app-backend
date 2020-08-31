// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Expiry
{

    public class WorkflowStats
    {
        public int Count { get; set; }
        public int TekCount { get; set; }
        public int Unauthorised { get; set; }
        public int Authorised { get; set; }
        public int AuthorisedAndFullyPublished { get; set; }
        public int TekPublished { get; set; }
        public int TekUnpublished { get; set; }
        public int Expired { get; set; }
    }
}