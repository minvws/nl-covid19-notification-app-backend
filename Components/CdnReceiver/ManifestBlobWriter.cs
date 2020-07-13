using System.IO;
using System.Net.Mime;
using Microsoft.Azure.Storage.Blob;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class ManifestBlobWriter : BlobWriterBase
    {
        public ManifestBlobWriter(IStorageAccountConfig storageAccountConfig, ILogger logger) : base(storageAccountConfig, logger)
        {
        }

        protected override BlobWriterResponse Write(CloudBlockBlob blob, MemoryStream input, ReceiveContentArgs content)
        {
            blob.Properties.ContentType = MediaTypeNames.Application.Zip;
            blob.Properties.CacheControl = "max-age=14400"; //TODO hard coded 4 hours.
            blob.UploadFromStream(input); //NB want to accept ANY change
            Logger.Debug($"Blob written - {blob.Uri}, CacheControl:{blob.Properties.CacheControl}, Overwritten:true.");
            return new BlobWriterResponse {Uri = blob.Uri, ItemAddedOrOverwritten = true};
        }
    }
}