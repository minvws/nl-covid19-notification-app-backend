using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DatabaseProvisioningTool
{
    public class App
    {
        private readonly ILogger<App> _Logger;
        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly ExposureContentDbContext _ExposureContentDbContext;

        public App(ILogger<App> logger, WorkflowDbContext workflowDbContext, ExposureContentDbContext exposureContentDbContext)
        {
            _Logger = logger;
            _WorkflowDbContext = workflowDbContext;
            _ExposureContentDbContext = exposureContentDbContext;
        }

        public async Task Run()
        {
            _Logger.LogInformation("Running...");
            
            _Logger.LogInformation("Apply WorkflowDb Migrations...");
            await _WorkflowDbContext.Database.EnsureCreatedAsync();
            
            _Logger.LogInformation("Apply ExposureContentDb Migrations...");
            await _ExposureContentDbContext.Database.EnsureCreatedAsync();
            
            _Logger.LogInformation("Completed...");
        }
    }
}
