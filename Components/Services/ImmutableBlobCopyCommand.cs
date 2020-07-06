using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Shared.Protocol;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
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

        public ImmutableBlobCopyCommand(IStorageAccountConfig sourceStorageAccountConfig, IStorageAccountConfig destinationStorageAccountConfig)
        {
            _SourceStorageAccountConfig = sourceStorageAccountConfig;
            _DestinationStorageAccountConfig = destinationStorageAccountConfig;
        }

        /// <summary>
        /// NB Not handling case where source immutable blob is deleted at same time as copy.
        /// NB Not using GetBlobReferenceFromServer as this requires a lot of complex exception handling.
        /// </summary>
        /// <param name="relativePath"></param>
        public async Task Execute(string relativePath)
        {
            try
            {
                var source = _SourceStorageAccountConfig.GetBlobDirect(relativePath, AccessCondition.GenerateIfExistsCondition());

                var destination = _DestinationStorageAccountConfig.GetBlobIndirect(relativePath);

                if (destination.Exists())
                    return;

                destination.StartCopy(source);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}