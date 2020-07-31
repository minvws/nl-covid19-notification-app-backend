// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration
{
    public class ProvisionDatabasesCommand
    {
        private readonly WorkflowDatabaseCreateCommand _Workflow;
        private readonly ContentDatabaseCreateCommand _Content;
        private readonly PublishingJobDatabaseCreateCommand _Job;
        private readonly ILogger _Logger;

        public ProvisionDatabasesCommand(WorkflowDatabaseCreateCommand workflow, ContentDatabaseCreateCommand content, PublishingJobDatabaseCreateCommand job, ILogger<ProvisionDatabasesCommand> logger)
        {
            _Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
            _Content = content ?? throw new ArgumentNullException(nameof(content));
            _Job = job ?? throw new ArgumentNullException(nameof(job));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute(string[] args)
        {
            var nuke = !args.Contains("nonuke");

            _Logger.LogInformation("Start.");

            _Logger.LogInformation("Workflow...");
            await _Workflow.Execute(nuke);

            if (!args.Contains("noworkflowdata"))
                await _Workflow.AddExampleContent();

            _Logger.LogInformation("Content...");
            await _Content.Execute(nuke);
    
            if (!args.Contains("nocontentdata"))
                await _Content.AddExampleContent();

            _Logger.LogInformation("Job...");
            await _Job.Execute(nuke);

            _Logger.LogInformation("Complete.");
        }
    }
}
