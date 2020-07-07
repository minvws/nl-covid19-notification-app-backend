using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    //public class StorageClientEx
    //{
    //    private readonly CloudBlobClient _BlobClient;

    //    public StorageClientEx(IStorageAccountConfig storageAccountConfig)
    //    {
    //        var c = CloudStorageAccount.Parse(storageAccountConfig.ConnectionString);
    //        _BlobClient = new CloudBlobClient(c.BlobEndpoint, c.Credentials);
    //    }

    //    public void Write(MemoryStream input, string containerName, string blobName)
    //    {
    //        var container = _BlobClient.GetContainerReference(containerName);
    //        var blob = container.GetBlockBlobReference(blobName);
    //        blob.UploadFromStream(input);
    //    }

    //    public async Task DeleteAsync(string containerName, string blobName)
    //    {
    //        var container = _BlobClient.GetContainerReference(containerName);
    //        var blob = container.GetBlockBlobReference(blobName);
    //        await blob.DeleteAsync();
    //    }
    //}
}