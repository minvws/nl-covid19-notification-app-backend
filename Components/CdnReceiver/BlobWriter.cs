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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using ProtoBuf;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class BlobWriterResponse
    { 
        public bool ItemAddedOrOverwritten { get; set; }
        public Uri Uri { get; set; }
    }

    public abstract class BlobWriterBase : IBlobWriter
    {
        private readonly IStorageAccountConfig _StorageAccountConfig;
        public BlobWriterBase(IStorageAccountConfig storageAccountConfig)
        {
            _StorageAccountConfig = storageAccountConfig;
        }

        public async Task<BlobWriterResponse> Write(ReceiveContentArgs content, DestinationArgs destination)
        {
            var blob = GetBlob(destination);
            using var buffer = new MemoryStream(content.SignedContent);
            return Write(blob, buffer, content);
            //return new BlobWriterResponse {ItemAddedOrOverwritten = true, Uri = blob.Uri};
        }

        protected abstract BlobWriterResponse Write(CloudBlockBlob blob, MemoryStream buffer, ReceiveContentArgs content);


        //TODO where is the GetBlockBlobReference(uri) api?
        private CloudBlockBlob GetBlob(DestinationArgs args)
        {
            var c = CloudStorageAccount.Parse(_StorageAccountConfig.ConnectionString);
            var blobClient = new CloudBlobClient(c.BlobStorageUri, c.Credentials);

            var splits = args.Path.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length == 0)
                throw new InvalidOperationException(); //TODO description

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
