// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class ProvisionDatabasesCommand
    {
        private readonly CreateWorkflowDatabase _Workflow;
        private readonly CreateContentDatabase _Content;

        public ProvisionDatabasesCommand(CreateWorkflowDatabase workflow, CreateContentDatabase content)
        {
            _Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
            _Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public async Task Execute()
        {
            await _Workflow.Execute();
            await _Workflow.AddExampleContent();

            await _Content.Execute();
            await _Content.AddExampleContent();
            Console.WriteLine("Completed.");
        }
    }
}
