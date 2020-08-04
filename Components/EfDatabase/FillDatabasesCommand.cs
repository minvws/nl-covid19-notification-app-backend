using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class FillDatabasesCommand
    {
        private readonly WorkflowDatabaseCreateCommand _Workflow;
        private readonly ContentDatabaseCreateCommand _Content;
        private readonly ILogger _Logger;

        public FillDatabasesCommand(WorkflowDatabaseCreateCommand workflow, ContentDatabaseCreateCommand content, ILogger<FillDatabasesCommand> logger)
        {
            _Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
            _Content = content ?? throw new ArgumentNullException(nameof(content));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute()
        {
            _Logger.LogInformation("Start.");

            _Logger.LogInformation("Workflow...");
            await _Workflow.AddExampleContent();

            _Logger.LogInformation("Content...");
            await _Content.AddExampleContent();

            _Logger.LogInformation("Complete.");
        }
    }
}