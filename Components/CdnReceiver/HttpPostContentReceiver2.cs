// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class HttpPostContentReceiver2
    {
        private readonly IBlobWriter _BlobWriter;
        private readonly IQueueSender<StorageAccountSyncMessage> _QueueSender;
        private readonly ILogger _Logger;

        public HttpPostContentReceiver2(IBlobWriter blobWriter, IQueueSender<StorageAccountSyncMessage> queueSender, ILogger<HttpPostContentReceiver2> logger)
        {
            _BlobWriter = blobWriter ?? throw new ArgumentNullException(nameof(blobWriter));
            _QueueSender = queueSender ?? throw new ArgumentNullException(nameof(queueSender));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Execute(ReceiveContentArgs content, DestinationArgs destination)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            //This was begging for a Try...
            var result = await _BlobWriter.Write(content, destination);

            if (!result.ItemAddedOrOverwritten)
            {
                _Logger.LogInformation("Content Added Or Overwritten.");
                return new ConflictResult();
            }

            var path = destination.Name.Equals(EndPointNames.ManifestName) 
                ? string.Concat(destination.Path, "/", EndPointNames.ManifestName)
                : string.Concat(destination.Path, "/", content.PublishingId);

            var mutable = destination.Name.Equals(EndPointNames.ManifestName);

            //Disambiguate sources of 404 errors
            try
            {
                _Logger.LogDebug($"Sending to queue - Path: {path} Mutable:{mutable}.");
                await _QueueSender.Send(new StorageAccountSyncMessage { RelativePath = path, MutableContent = mutable });
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.ToString());
                return new StatusCodeResult(500);
            }
            return new OkResult();
        }
    }
}