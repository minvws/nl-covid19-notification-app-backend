using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Shared.Protocol;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;

namespace CdnRegionSync
{
    /// <summary>
    /// All blobs except manifest
    /// Manifest uses MutableBlobCopyCommand
    /// </summary>
    public class ImmutableBlobCopyCommand
    {
        private readonly IStorageAccountConfig _SourceStorageAccountConfig;
        private readonly IStorageAccountConfig _DestinationStorageAccountConfig;
        private readonly ILogger _Logger;

        public ImmutableBlobCopyCommand(IStorageAccountConfig sourceStorageAccountConfig, IStorageAccountConfig destinationStorageAccountConfig, ILogger logger)
        {
            _SourceStorageAccountConfig = sourceStorageAccountConfig ?? throw new ArgumentNullException(nameof(sourceStorageAccountConfig));
            _DestinationStorageAccountConfig = destinationStorageAccountConfig ?? throw new ArgumentNullException(nameof(destinationStorageAccountConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// NB Not handling case where source immutable blob is deleted at same time as copy.
        /// NB Not using GetBlobReferenceFromServer as this requires a lot of complex exception handling.
        /// </summary>
        /// <param name="relativePath"></param>
        public async Task Execute(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException(nameof(relativePath));

            _Logger.LogDebug($"Source storage account: {CloudStorageAccount.Parse(_SourceStorageAccountConfig.ConnectionString).BlobEndpoint.AbsoluteUri}.");
            _Logger.LogDebug($"Destination storage account: {CloudStorageAccount.Parse(_DestinationStorageAccountConfig.ConnectionString).BlobEndpoint.AbsoluteUri}.");
            _Logger.LogDebug($"Copying {relativePath}.");

#if DEBUG
            try
            {
#endif
                var source = _SourceStorageAccountConfig.GetBlobDirect(relativePath, AccessCondition.GenerateIfExistsCondition());
                if (source == null)
                {
                    _Logger.LogError($"Could not make reference to source blob: {relativePath}.");
                    return;
                }

                var destination = _DestinationStorageAccountConfig.GetBlobIndirect(relativePath);
                if (destination == null)
                {
                    _Logger.LogError($"Could not make reference to destination blob: {relativePath}.");
                    return;
                }

                if (destination.Exists())
                {
                    _Logger.LogInformation("Destination blob already exists.");
                    return;
                }

                _Logger.LogInformation("Copying to new blob.");
                destination.StartCopy(source);
                _Logger.LogInformation("Copied.");
#if DEBUG
            }
            catch (Exception e)
            {
                throw;
            }
#endif
        }
    }
}