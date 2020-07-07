// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class PusherTask
    {
        private readonly IDataApiUrls _DataApiConfig;
        private readonly IReceiverConfig _ReceiverConfig;
        private readonly ILogger<PusherTask> _Logger;
        private readonly IJsonSerializer _JsonSerializer;

        public PusherTask(ILogger<PusherTask> logger, IDataApiUrls dataApiConfig, IReceiverConfig receiverConfig, IJsonSerializer jsonSerializer)
        {
            _Logger = logger;
            _ReceiverConfig = receiverConfig;
            _DataApiConfig = dataApiConfig;
            _JsonSerializer = jsonSerializer;
        }

        public async Task PushIt()
        {
            _Logger.LogInformation("Running.");

            //Read manifest
            var bcr = await GetContent(_DataApiConfig.Manifest);
            var manifestJson = Encoding.UTF8.GetString(bcr.Content); //NB this is why we retain the JSON version
            var manifest = _JsonSerializer.Deserialize<ManifestContent>(manifestJson);

            if (manifest == null)
                throw new InvalidOperationException("Unable to read manifest.");

            _Logger.LogInformation("Read Manifest.");

            bool writtenToDb;
            //Push all manifest items
            if (!string.IsNullOrWhiteSpace(manifest.AppConfig))
            {
                writtenToDb = await PushItGood(string.Format(_DataApiConfig.AppConfig, manifest.AppConfig), _ReceiverConfig.AppConfig);
                _Logger.LogInformation($"Pushed AppConfig {manifest.AppConfig} - New item:{writtenToDb}.");
            }

            if (!string.IsNullOrWhiteSpace(manifest.ResourceBundle))
            {
                writtenToDb = await PushItGood(string.Format(_DataApiConfig.ResourceBundle, manifest.ResourceBundle), _ReceiverConfig.ResourceBundle);
                _Logger.LogInformation($"Pushed ResourceBundle {manifest.ResourceBundle} - New item:{writtenToDb}.");
            }

            if (!string.IsNullOrWhiteSpace(manifest.RiskCalculationParameters))
            {
                writtenToDb = await PushItGood(string.Format(_DataApiConfig.RiskCalculationParameters, manifest.RiskCalculationParameters), _ReceiverConfig.RiskCalculationParameters);
                _Logger.LogInformation($"Pushed RiskCalculationParameters {manifest.RiskCalculationParameters} - New item:{writtenToDb}.");
            }

            foreach (var i in manifest.ExposureKeySets)
            {
                if (!string.IsNullOrWhiteSpace(i))
                {
                    writtenToDb = await PushItGood(string.Format(_DataApiConfig.ExposureKeySet, i), _ReceiverConfig.ExposureKeySet);
                    _Logger.LogInformation($"Pushed ExposureKeySet {i} - New item:{writtenToDb}.");
                }
            }

            writtenToDb = await new SubKeyAuthPostBytesToUrl(_ReceiverConfig).Execute(_ReceiverConfig.Manifest, MapSignedContent(bcr));
            _Logger.LogInformation($"Pushed manifest - New item:{writtenToDb}.");

            _Logger.LogInformation("Completed.");
        }

        private async Task<bool> PushItGood(string fromUri, string toUri)
        {
            var bcr = await GetContent(fromUri);
            return await new SubKeyAuthPostBytesToUrl(_ReceiverConfig).Execute(toUri, MapSignedContent(bcr));
        }

        private async Task<BinaryContentResponse> GetContent(string fromUri)
        {
            var content = await new BasicAuthDataApiReader(_DataApiConfig).Read(fromUri);
            var contentBytes = Encoding.UTF8.GetString(content);
            return _JsonSerializer.Deserialize<BinaryContentResponse>(contentBytes);
        }

        private byte[] MapSignedContent(BinaryContentResponse bcr)
        {
            var args = new ReceiveContentArgs
            {
                LastModified = bcr.LastModified,
                SignedContent = bcr.SignedContent,
                PublishingId = bcr.PublishingId,
            };
            var argsString = _JsonSerializer.Serialize(args);
            return Encoding.UTF8.GetBytes(argsString);
        }
    }
}
