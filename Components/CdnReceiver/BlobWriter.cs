using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using ProtoBuf;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class BlobWriter
    {
        private readonly IStorageAccountConfig _StorageAccountConfig;
        private CloudBlobClient _BlobClient;

        public BlobWriter(IStorageAccountConfig storageAccountConfig)
        {
            _StorageAccountConfig = storageAccountConfig;
        }

        public async Task ReceiveManifest(string path, string name, byte[] contents, DateTime published)
        {
            InitBlobClient();
            var blob = GetBlob(path, name);
            try
            {
                using var input = new MemoryStream(contents);
                blob.Properties.ContentType = MediaTypeNames.Application.Zip;
                blob.Properties.CacheControl = "max-age=14400"; //TODO hard coded 4 hours.
                blob.UploadFromStream(input, AccessCondition.GenerateIfNotModifiedSinceCondition(published));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void InitBlobClient()
        {
            var c = CloudStorageAccount.Parse(_StorageAccountConfig.ConnectionString);
            _BlobClient = new CloudBlobClient(c.BlobStorageUri, c.Credentials);
        }

        public async Task ReceiveOtherContent(string path, string name, byte[] contents, DateTime _)
        {
            InitBlobClient();
            var blob = GetBlob(path, name);
            try
            {
                using var input = new MemoryStream(contents);
                blob.Properties.ContentType = MediaTypeNames.Application.Zip;
                blob.Properties.CacheControl = "immutable;max-age=31536000"; //TODO hard coded 1 year.
                blob.UploadFromStream(input, AccessCondition.GenerateIfNotExistsCondition());
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        //TODO where is the GetBlockBlobReference(uri) api?
        private CloudBlockBlob GetBlob(string path, string name)
        {
            var splits = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length == 0)
                throw new InvalidOperationException(); //TODO description

            var container = _BlobClient.GetContainerReference(splits[0]);
            
            if (splits.Length == 1)
                return container.GetBlockBlobReference(name);

            var dir = container.GetDirectoryReference(splits[1]);
            for (var i = 2; i < splits.Length; i++)
                dir = dir.GetDirectoryReference(splits[i]);

            return dir.GetBlockBlobReference(name);
        }
    }
}
