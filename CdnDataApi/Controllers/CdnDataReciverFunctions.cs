using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    [ApiController]
    [Route("[controller]")]
    public class CdnDataReciverFunctions
    {
        [HttpPost]
        [Route(EndPointNames.CdnApi.Manifest)]
        public async Task<IActionResult> HttpPostManifest(HttpRequest httpRequest, [FromServices] HttpPostContentReciever<ManifestEntity> command)
            => await command.Execute(httpRequest);

        [HttpPost]
        [Route(EndPointNames.CdnApi.AppConfig)]
        public async Task<IActionResult> HttpPostAppConfig(HttpRequest httpRequest, [FromServices] HttpPostContentReciever<AppConfigContentEntity> command)
            => await command.Execute(httpRequest);

        [HttpPost]
        [Route(EndPointNames.CdnApi.ResourceBundle)]
        public async Task<IActionResult> HttpPostResourceBundle(HttpRequest httpRequest, [FromServices] HttpPostContentReciever<ResourceBundleContentEntity> command)
            => await command.Execute(httpRequest);

        [HttpPost]
        [Route(EndPointNames.CdnApi.RiskCalculationParameters)]
        public async Task<IActionResult> HttpPostCalcConfig(HttpRequest httpRequest, [FromServices] HttpPostContentReciever<RiskCalculationContentEntity> command)
            => await command.Execute(httpRequest);

        [HttpPost]
        [Route(EndPointNames.CdnApi.ExposureKeySet)]
        public async Task<IActionResult> HttpPostEks(HttpRequest httpRequest, [FromServices] HttpPostContentReciever<ExposureKeySetContentEntity> command)
            => await command.Execute(httpRequest);

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