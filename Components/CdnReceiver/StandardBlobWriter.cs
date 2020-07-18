using System.IO;
using System.Net.Mime;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class StandardBlobWriter : BlobWriterBase
    {
        public StandardBlobWriter(IStorageAccountConfig storageAccountConfig, ILogger<StandardBlobWriter> logger) : base(storageAccountConfig, logger)
        {
        }

        protected override BlobWriterResponse Write(CloudBlockBlob blob, MemoryStream input, ReceiveContentArgs content)
        {
            Logger.LogInformation($"Blob writing - Uri: {blob.Uri}");

            blob.Properties.ContentType = MediaTypeNames.Application.Zip;
            blob.Properties.CacheControl = "immutable;max-age=31536000"; //TODO hard coded 1 year.
            try
            {
                blob.UploadFromStream(input, AccessCondition.GenerateIfNotExistsCondition());
                Logger.LogInformation($"Blob written - Uri: {blob.Uri}, CacheControl:{blob.Properties.CacheControl}, Overwritten:true.");
                return new BlobWriterResponse { Uri = blob.Uri, ItemAddedOrOverwritten = true };
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409)
                {
                    Logger.LogInformation($"Blob written - Uri: {blob.Uri}, CacheControl:{blob.Properties.CacheControl}, Overwritten:false.");
                    return new BlobWriterResponse {Uri = blob.Uri, ItemAddedOrOverwritten = false};
                }

                throw; //Leave this to default handler
            }
            
        }
    }
}