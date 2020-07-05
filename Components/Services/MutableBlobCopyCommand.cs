using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace CdnRegionSync
{
    /// <summary>
    /// For the manifest
    /// </summary>
    public class MutableBlobCopyCommand
    {
        private readonly IStorageAccountConfig _SourceStorageAccountConfig;
        private readonly IStorageAccountConfig _DestinationStorageAccountConfig;
        private CloudBlockBlob? _Source;
        private CloudBlockBlob? _Destination;

        public MutableBlobCopyCommand(IStorageAccountConfig sourceStorageAccountConfig, IStorageAccountConfig destinationStorageAccountConfig)
        {
            _SourceStorageAccountConfig = sourceStorageAccountConfig;
            _DestinationStorageAccountConfig = destinationStorageAccountConfig;
        }

        public async Task Execute(string relativePath)
        {
            _Source = _SourceStorageAccountConfig.GetBlobDirect(relativePath);

            if (_Source == null)
                return; //TODO log

            try
            {
                await _Source.AcquireLeaseAsync(null); //Lease Id not used - cannot lease the source blob of a copy

                if (!_Source.Exists())
                    return; //TODO log

                _Destination = _DestinationStorageAccountConfig.GetBlobIndirect(relativePath);

                if (!_Destination.Exists())
                { 
                    //Create on first copy
                    _Destination.StartCopy(_Source);
                    return; //TODO log
                }

                //Update
                if (await _Source.ContentSame(_Destination))
                    return; //TODO log - already updated

                var destinationLease = await _Destination.AcquireLeaseAsync(null);
                _Destination.StartCopy(_Source, destAccessCondition: AccessCondition.GenerateLeaseCondition(destinationLease));
            }  
            finally
            {
                await _Source.ReleaseLease();
                await _Destination.ReleaseLease();
            }
        }
    }
}