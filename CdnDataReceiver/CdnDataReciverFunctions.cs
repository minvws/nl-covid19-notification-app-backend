using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class CdnDataReciverFunctions : AppFunctionBase
    {
        [FunctionName("manifest")]
        public async Task<IActionResult> HttpPostManifest([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/manifest")] HttpRequest httpRequest, ILogger logger, ExecutionContext executionContext)
        {
            SetConfig(executionContext);
            return await new Receiver<ManifestEntity>().Execute(httpRequest, logger, Configuration);
        }

        [FunctionName("appconfig")]
        public async Task<IActionResult> HttpPostAppConfig([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/appconfig")] HttpRequest httpRequest, ILogger logger, ExecutionContext executionContext)
        {
            SetConfig(executionContext);
            return await new Receiver<AppConfigContentEntity>().Execute(httpRequest, logger, Configuration);
        }

        [FunctionName("resourcebundle")]
        public async Task<IActionResult> HttpPostResourceBundle([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/resourcebundle")] HttpRequest httpRequest, ILogger logger, ExecutionContext executionContext)
        {
            SetConfig(executionContext);
            return await new Receiver<ResourceBundleContentEntity>().Execute(httpRequest, logger, Configuration);
        }

        [FunctionName("riskcalculationparameters")]
        public async Task<IActionResult> HttpPostCalcConfig([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/riskcalculationparameters")] HttpRequest httpRequest, ILogger logger, ExecutionContext executionContext)
        {
            SetConfig(executionContext);
            return await new Receiver<RiskCalculationContentEntity>().Execute(httpRequest, logger, Configuration);
        }

        [FunctionName("exposurekeyset")]
        public async Task<IActionResult> HttpPostEks([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/exposurekeyset")] HttpRequest httpRequest, ILogger logger, ExecutionContext executionContext)
        {
            SetConfig(executionContext);
            return await new Receiver<ExposureKeySetContentEntity>().Execute(httpRequest, logger, Configuration);
        }

        //[FunctionName("provision")]
        //public async Task<IActionResult> HttpPostProvisionDb([HttpTrigger(AuthorizationLevel.Function, "post", Route = "/v1/nukeandpave")] HttpRequest httpRequest, ILogger logger, ExecutionContext executionContext)
        //{
        //    SetConfig(executionContext);
        //    var config = new StandardEfDbConfig(Configuration, "Content");
        //    var builder = new SqlServerDbContextOptionsBuilder(config);
        //    var dbContext = new ExposureContentDbContext(builder.Build());
        //    await dbContext.Database.EnsureDeletedAsync();
        //    await dbContext.Database.EnsureCreatedAsync();
        //    return new OkResult();
        //}
    }
}