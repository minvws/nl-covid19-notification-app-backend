// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class TekWriteArgs
    {
        public TekReleaseWorkflowStateEntity WorkflowStateEntityEntity { get; set; }
        public Tek[] NewItems { get; set; }
    }
}
