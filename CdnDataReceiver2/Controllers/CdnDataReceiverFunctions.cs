using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using Serilog;

namespace CdnDataReceiver.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CdnDataReceiverFunctions : ControllerBase
    {
        private readonly IContentPathProvider _ContentPathProvider;
        private readonly ILogger _Logger;

        public CdnDataReceiverFunctions(IContentPathProvider contentPathProvider, ILogger logger)
        {
            _ContentPathProvider = contentPathProvider ?? throw new ArgumentNullException(nameof(contentPathProvider));
            _Logger = logger;
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.Manifest)]
        public async Task<IActionResult> HttpPostManifest([FromBody] ReceiveContentArgs args, [FromServices]ManifestBlobWriter blobWriter, [FromServices]IQueueSender<StorageAccountSyncMessage> qSender)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (blobWriter == null) throw new ArgumentNullException(nameof(blobWriter));
            if (qSender == null) throw new ArgumentNullException(nameof(qSender));

            var command = new HttpPostContentReceiver2(blobWriter, qSender, _Logger);
            return await command.Execute(args, new DestinationArgs {Path = _ContentPathProvider.Manifest, Name = EndPointNames.ManifestName});
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.AppConfig)]
        public async Task<IActionResult> HttpPostAppConfig([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReceiver2 command)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (command == null) throw new ArgumentNullException(nameof(command));

            return await command.Execute(args, new DestinationArgs {Path = _ContentPathProvider.AppConfig, Name = args.PublishingId});
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.ResourceBundle)]
        public async Task<IActionResult> HttpPostResourceBundle([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReceiver2 command)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (command == null) throw new ArgumentNullException(nameof(command));

            return await command.Execute(args, new DestinationArgs {Path = _ContentPathProvider.ResourceBundle, Name = args.PublishingId});
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.RiskCalculationParameters)]
        public async Task<IActionResult> HttpPostCalcConfig([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReceiver2 command)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (command == null) throw new ArgumentNullException(nameof(command));

            return await command.Execute(args, new DestinationArgs {Path = _ContentPathProvider.RiskCalculationParameters, Name = args.PublishingId});
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.ExposureKeySet)]
        public async Task<IActionResult> HttpPostEks([FromBody] ReceiveContentArgs args, [FromServices] HttpPostContentReceiver2 command)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (command == null) throw new ArgumentNullException(nameof(command));

            return await command.Execute(args, new DestinationArgs {Path = _ContentPathProvider.ExposureKeySet, Name = args.PublishingId});
        }
    }
}