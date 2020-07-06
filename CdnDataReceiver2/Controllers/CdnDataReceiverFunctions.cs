using System;
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
    public class CdnDataReceiverFunctions : ControllerBase
    {
        private readonly IContentPathProvider _ContentPathProvider;

        public CdnDataReceiverFunctions(IContentPathProvider contentPathProvider)
        {
            _ContentPathProvider = contentPathProvider;
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.Manifest)]
        public async Task<IActionResult> HttpPostManifest([FromBody] ReceiveContentArgs args, [FromServices]ManifestBlobWriter blobWriter, [FromServices]IQueueSender<StorageAccountSyncMessage> qSender)
        {
            var command = new HttpPostContentReciever2(blobWriter, qSender);
            return await command.Execute(args, new DestinationArgs {Path = _ContentPathProvider.Manifest, Name = EndPointNames.ManifestName});
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.AppConfig)]
        public async Task<IActionResult> HttpPostAppConfig([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs {Path = _ContentPathProvider.AppConfig, Name = args.PublishingId });

        [HttpPost]
        [Route(EndPointNames.CdnApi.ResourceBundle)]
        public async Task<IActionResult> HttpPostResourceBundle([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs { Path = _ContentPathProvider.ResourceBundle, Name = args.PublishingId });

        [HttpPost]
        [Route(EndPointNames.CdnApi.RiskCalculationParameters)]
        public async Task<IActionResult> HttpPostCalcConfig([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs { Path = _ContentPathProvider.RiskCalculationParameters, Name = args.PublishingId });

        [HttpPost]
        [Route(EndPointNames.CdnApi.ExposureKeySet)]
        public async Task<IActionResult> HttpPostEks([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReciever2 command)
            => await command.Execute(args, new DestinationArgs { Path = _ContentPathProvider.ExposureKeySet, Name = args.PublishingId });
    }
}