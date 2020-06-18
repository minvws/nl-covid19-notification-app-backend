// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class WorkflowConfig : AppSettingsReader, IWorkflowConfig
    {
        private const int WorkflowTokenTtlDaysDefault = 14;

        public WorkflowConfig(IConfiguration config) : base(config, "Workflow")
        {
        }

        public double SecretLifetimeDays => GetValueDouble(nameof(SecretLifetimeDays), WorkflowTokenTtlDaysDefault);
        public double AuthorisationWindowDurationMinutes => GetValueDouble(nameof(AuthorisationWindowDurationMinutes), WorkflowTokenTtlDaysDefault);
    }
}