// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using Org.BouncyCastle.Utilities.Encoders;
using Serilog.Formatting.Raw;
using SQLitePCL;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class PushIt
    {
        private readonly IDataApiUrls _DataApiConfig;
        private readonly IReceiverConfig _ReceiverConfig;

        private readonly ILogger<PushIt> _Logger;

        public PushIt(ILogger<PushIt> logger, IDataApiUrls dataApiConfig, IReceiverConfig receiverConfig)
        {
            _Logger = logger;
            _ReceiverConfig = receiverConfig;
            _DataApiConfig = dataApiConfig;
        }

        public async Task Run()
        {
            _Logger.LogInformation("Running...");

            //Read manifest
            var wc = new WebClient();
            wc.Headers.Add("accept", MediaTypeNames.Application.Json);
            var contentJsonBytes = await wc.DownloadDataTaskAsync(_DataApiConfig.Manifest);
            
            //Push manifest
            if (new PushContentByUrl().Execute(_ReceiverConfig.Manifest, contentJsonBytes))
            {
                _Logger.LogInformation($"Pushed manifest.");
            }
            else
            {
                _Logger.LogInformation("Completed (Up to date).");
                return;
            }

            //Unpack manifest
            var contentBytes = Encoding.UTF8.GetString(contentJsonBytes);
            var bcr = JsonConvert.DeserializeObject<BinaryContentResponse>(contentBytes);

            var manifestJsonBytes = Base64.Decode(bcr.Content);
            var manifestJson = Encoding.UTF8.GetString(manifestJsonBytes);
            var manifest = JsonConvert.DeserializeObject<ManifestContent>(manifestJson);

            //Push all manifest items
            await Push(string.Format(_DataApiConfig.AppConfig, manifest.AppConfig), _ReceiverConfig.AppConfig);
            _Logger.LogInformation($"Pushed AppConfig {manifest.AppConfig}.");
            await Push(string.Format(_DataApiConfig.ResourceBundle, manifest.ResourceBundle), _ReceiverConfig.ResourceBundle);
            _Logger.LogInformation($"Pushed ResourceBundle {manifest.ResourceBundle}.");
            await Push(string.Format(_DataApiConfig.RiskCalculationParameters, manifest.RiskCalculationParameters), _ReceiverConfig.RiskCalculationParameters);
            _Logger.LogInformation($"Pushed RiskCalculationParameters {manifest.RiskCalculationParameters}.");

            foreach (var i in manifest.ExposureKeySets)
            {
                await Push(string.Format(_DataApiConfig.ExposureKeySet, i), _ReceiverConfig.ExposureKeySet);
                _Logger.LogInformation($"Pushed ExposureKeySet {i}.");
            }

            _Logger.LogInformation("Completed...");
        }

        private async Task Push(string fromUri, string toUri)
        {
            var content = await new BasicAuthDataApiReader(_DataApiConfig).Read(fromUri);
            new PushContentByUrl().Execute(toUri, content);
        }
    }
}
