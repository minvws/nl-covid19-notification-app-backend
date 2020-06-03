// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public class WorkflowValidationAppSettings : AppSettingsReader, IWorkflowValidatorConfig
    {
        public WorkflowValidationAppSettings(IConfiguration config) : base(config)
        {
        }
        public int WorkflowKeyCountMin => GetValueInt32("Validation:AgWorkflows:ItemCountMin", 1);
        public int WorkflowKeyCountMax => GetValueInt32("Validation:AgWorkflows:ItemCountMax", 21);
    }
}