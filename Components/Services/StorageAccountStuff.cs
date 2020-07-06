using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;

namespace CdnRegionSync
{
    public static class StorageAccountStuff
    {
        public static async Task<bool> ContentSame(this CloudBlockBlob left, CloudBlockBlob right)
        {
            await left.FetchAttributesAsync();
            await right.FetchAttributesAsync();
            return left.Properties.ContentMD5 == right.Properties.ContentMD5;
        }

        public static async Task ReleaseLease(this CloudBlockBlob? item)
        {
            if (item == null || !item.Exists())
                return;

            await item.FetchAttributesAsync();
            if (item.Properties.LeaseState != LeaseState.Available)
                await item.BreakLeaseAsync(TimeSpan.Zero);
        }

        /// <summary>
        /// Use for source blobs
        /// </summary>
        public static CloudBlockBlob GetBlobIndirect(this IStorageAccountConfig config, string relativePath)
        {
            var destinationAccount = CloudStorageAccount.Parse(config.ConnectionString);
            var destinationBlobClient = destinationAccount.CreateCloudBlobClient();
            var containerName = relativePath.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];
            var path = string.Join("/", relativePath.Split("/", StringSplitOptions.RemoveEmptyEntries).Skip(1));
            var containerReference = destinationBlobClient.GetContainerReference(containerName);
            return containerReference.GetBlockBlobReference(path);
        }

        /// <summary>
        /// NB Not using GetBlobReferenceFromServer for destination as this requires a lot of complex exception handling.
        /// </summary>
        public static CloudBlockBlob? GetBlobDirect(this IStorageAccountConfig config, string relativePath, AccessCondition? ac = null)
        {
            var sourceAccount = CloudStorageAccount.Parse(config.ConnectionString);
            var sourceBlobClient = sourceAccount.CreateCloudBlobClient();
            return sourceBlobClient.GetBlobReferenceFromServer(
                new StorageUri(new Uri(sourceBlobClient.BaseUri, relativePath)), ac) as CloudBlockBlob;
        }
    }
}