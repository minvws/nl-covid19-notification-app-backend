// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public interface IBlobWriter
    {
        Task<BlobWriterResponse> Write(ReceiveContentArgs content, DestinationArgs destination);
    }

    public class ManifestBlobWriter : BlobWriterBase
    {
        public ManifestBlobWriter(IStorageAccountConfig storageAccountConfig) : base(storageAccountConfig)
        {
        }

        protected override void Write(CloudBlockBlob blob, MemoryStream input, ReceiveContentArgs content)
        {
            blob.Properties.ContentType = MediaTypeNames.Application.Zip;
            blob.Properties.CacheControl = "max-age=14400"; //TODO hard coded 4 hours.
            blob.UploadFromStream(input, AccessCondition.GenerateIfNotModifiedSinceCondition(content.LastModified));
        }
    }

    public class StandardBlobWriter : BlobWriterBase
    {
        public StandardBlobWriter(IStorageAccountConfig storageAccountConfig) : base(storageAccountConfig)
        {
        }
        protected override void Write(CloudBlockBlob blob, MemoryStream input, ReceiveContentArgs content)
        {
            blob.Properties.ContentType = MediaTypeNames.Application.Zip;
            blob.Properties.CacheControl = "immutable;max-age=31536000"; //TODO hard coded 1 year.
            blob.UploadFromStream(input, AccessCondition.GenerateIfNotExistsCondition());
        }
    }


}