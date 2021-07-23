// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands
{
    public class RemoveExpiredWorkflowsResult : ICommandResult
    {
        public WorkflowStats Before { get; } = new WorkflowStats();
        public bool DeletionsOn { get; set; }
        public int GivenMercy { get; set; }
        public WorkflowStats After { get; } = new WorkflowStats();
        public bool HasErrors { get; set; }
    }
}
