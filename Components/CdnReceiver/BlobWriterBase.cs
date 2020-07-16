using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public abstract class BlobWriterBase : IBlobWriter
    {
        private readonly IStorageAccountConfig _StorageAccountConfig;
        protected ILogger Logger { get; }

        protected BlobWriterBase(IStorageAccountConfig storageAccountConfig, ILogger logger)
        {
            _StorageAccountConfig = storageAccountConfig ?? throw new ArgumentNullException(nameof(storageAccountConfig));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BlobWriterResponse> Write(ReceiveContentArgs content, DestinationArgs destination)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var blob = GetBlob(destination);
            using var buffer = new MemoryStream(content.SignedContent);
            return Write(blob, buffer, content);
        }

        protected abstract BlobWriterResponse Write(CloudBlockBlob blob, MemoryStream buffer, ReceiveContentArgs content);

        //TODO where is the GetBlockBlobReference(uri) api?
        private CloudBlockBlob GetBlob(DestinationArgs args)
        {
            var c = CloudStorageAccount.Parse(_StorageAccountConfig.ConnectionString);
            var blobClient = new CloudBlobClient(c.BlobStorageUri, c.Credentials);

            var splits = args.Path.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length == 0)
            {
                Logger.LogError($"Path invalid: {args.Path}");
                throw new InvalidOperationException("Path invalid.");
            }

            var container = blobClient.GetContainerReference(splits[0]);

            if (splits.Length == 1)
                return container.GetBlockBlobReference(args.Name);

            var dir = container.GetDirectoryReference(splits[1]);
            for (var i = 2; i < splits.Length; i++)
                dir = dir.GetDirectoryReference(splits[i]);

            return dir.GetBlockBlobReference(args.Name);
        }
    }
}