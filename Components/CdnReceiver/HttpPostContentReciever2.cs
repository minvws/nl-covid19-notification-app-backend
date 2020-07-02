// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{

    public class DestinationArgs
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }

    public class HttpPostContentReciever2
    {
        private readonly BlobWriter _BlobWriter;
        //private readonly QueueSendCommand<ContentSyncArgs> _QueueSendCommand;

        public HttpPostContentReciever2(BlobWriter blobWriter)
        {
            _BlobWriter = blobWriter;
        }

        public async Task<IActionResult> Execute(ReceiveContentArgs content, DestinationArgs destination)
        {
            if (content == null)
                return new OkResult();

            //Write to storage account
            if (destination.Name.Equals("manifest"))
                await _BlobWriter.ReceiveManifest(destination.Path, destination.Name, content.SignedContent, content.LastModified);
            else
                await _BlobWriter.ReceiveOtherContent(destination.Path, destination.Name, content.SignedContent, content.LastModified);

            //await _QueueSendCommand.Execute(args);

            return new OkResult();
        }
    }
}