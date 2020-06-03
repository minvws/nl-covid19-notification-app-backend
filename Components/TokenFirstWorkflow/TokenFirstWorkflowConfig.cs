// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow
{
    public class TokenFirstWorkflowConfig : AppSettingsReader, ITokenFirstWorkflowConfig
    {
        private const int WorkflowTokenTtlDaysDefault = 14;

        public TokenFirstWorkflowConfig(IConfiguration config) : base(config, "TokenFirstWorkflow")
        {
        }

        public double WorkflowTokenTtlDays => GetValueDouble(nameof(WorkflowTokenTtlDays), WorkflowTokenTtlDaysDefault);
        public double WorkflowWriteWindowDurationMinutes => GetValueDouble(nameof(WorkflowWriteWindowDurationMinutes), WorkflowTokenTtlDaysDefault);
    }
}