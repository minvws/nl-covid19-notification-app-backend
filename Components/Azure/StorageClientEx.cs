// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Azure
{
    public class StorageClientEx
    {
        private readonly CloudBlobClient _BlobClient;

        public StorageClientEx(IStorageAccountConfig storageAccountConfig)
        {
            var c = CloudStorageAccount.Parse(storageAccountConfig.ConnectionString);
            _BlobClient = new CloudBlobClient(c.BlobEndpoint, c.Credentials);
        }

        public void Write(Stream input, string containerName, string blobName)
        {
            var blob = GetBlobReference(containerName, blobName);
            blob.UploadFromStream(input);
        }

        private CloudBlockBlob GetBlobReference(string containerName, string blobName)
        {
            var container = _BlobClient.GetContainerReference(containerName);
            return container.GetBlockBlobReference(blobName);
        }

        public async Task DeleteAsync(string containerName, string blobName)
        {
            var blob = GetBlobReference(containerName, blobName);
            await blob.DeleteAsync();
        }
    }
}