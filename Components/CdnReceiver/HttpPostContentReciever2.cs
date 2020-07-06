// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{

    public class DestinationArgs
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }

    public class StorageAccountSyncMessage
    {
        /// <summary>
        /// DestinationArgs.Path + Name.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Manifest only = true
        /// </summary>
        public bool MutableContent { get; set; }
    }

    public class HttpPostContentReciever2
    {
        private readonly IBlobWriter _BlobWriter;
        private readonly IQueueSender<StorageAccountSyncMessage> _QueueSender;

        public HttpPostContentReciever2(IBlobWriter blobWriter, IQueueSender<StorageAccountSyncMessage> queueSender)
        {
            _BlobWriter = blobWriter;
            _QueueSender = queueSender;
        }

        public async Task<IActionResult> Execute(ReceiveContentArgs content, DestinationArgs destination)
        {
            //TODO deeper validation
            if (content == null || destination == null)
                return new OkResult();

            //This was begging for a Try...
            var result = await _BlobWriter.Write(content, destination);

            if (!result.ItemAddedOrOverwritten) 
                return new ConflictResult();

            var path = destination.Name.Equals(EndPointNames.ManifestName) 
                ? string.Concat(destination.Path, "/", EndPointNames.ManifestName)
                : string.Concat(destination.Path, "/", content.PublishingId);

            var mutable = destination.Name.Equals(EndPointNames.ManifestName);
            await _QueueSender.Send(new StorageAccountSyncMessage { RelativePath = path, MutableContent = mutable });
            return new OkResult();
        }
    }
}