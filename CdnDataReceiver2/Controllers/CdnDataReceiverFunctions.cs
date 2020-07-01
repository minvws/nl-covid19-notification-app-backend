using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace CdnDataReceiver.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    //[Authorize] //TODO
    public class CdnDataReceiverFunctions : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.CdnApi.Manifest)]
        public async Task<IActionResult> HttpPostManifest([FromBody]ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs{ Path = "vws/v01", Name = "manifest"}); //TODO all paths as settings.

        [HttpPost]
        [Route(EndPointNames.CdnApi.AppConfig)]
        public async Task<IActionResult> HttpPostAppConfig([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs {Path = "vws/v01/appconfig", Name = args.PublishingId });

        [HttpPost]
        [Route(EndPointNames.CdnApi.ResourceBundle)]
        public async Task<IActionResult> HttpPostResourceBundle([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs { Path = "vws/v01/resourcebundle", Name = args.PublishingId });

        [HttpPost]
        [Route(EndPointNames.CdnApi.RiskCalculationParameters)]
        public async Task<IActionResult> HttpPostCalcConfig([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs { Path = "vws/v01/riskcalculationparameters", Name = args.PublishingId });

        [HttpPost]
        [Route(EndPointNames.CdnApi.ExposureKeySet)]
        public async Task<IActionResult> HttpPostEks([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs { Path = "vws/v01/exposurekeyset", Name = args.PublishingId });
    }
}