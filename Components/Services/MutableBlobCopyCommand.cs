using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;

namespace CdnRegionSync
{
    /// <summary>
    /// For the manifest.
    /// </summary>
    public class MutableBlobCopyCommand
    {
        private readonly IStorageAccountConfig _SourceStorageAccountConfig;
        private readonly IStorageAccountConfig _DestinationStorageAccountConfig;
        private readonly ILogger _Logger;
        private CloudBlockBlob? _Source;
        private CloudBlockBlob? _Destination;

        public MutableBlobCopyCommand(IStorageAccountConfig sourceStorageAccountConfig, IStorageAccountConfig destinationStorageAccountConfig, ILogger<MutableBlobCopyCommand> logger)
        {
            _SourceStorageAccountConfig = sourceStorageAccountConfig ?? throw new ArgumentNullException(nameof(sourceStorageAccountConfig));
            _DestinationStorageAccountConfig = destinationStorageAccountConfig ?? throw new ArgumentNullException(nameof(destinationStorageAccountConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException(nameof(relativePath));

            _Logger.LogDebug($"Source storage account: {CloudStorageAccount.Parse(_SourceStorageAccountConfig.ConnectionString).BlobEndpoint.AbsoluteUri}");
            _Logger.LogDebug($"Destination storage account: {CloudStorageAccount.Parse(_DestinationStorageAccountConfig.ConnectionString).BlobEndpoint.AbsoluteUri}");
            _Logger.LogDebug($"Copying {relativePath}.");

            _Source = _SourceStorageAccountConfig.GetBlobDirect(relativePath);

            if (_Source == null)
            {
                _Logger.LogError($"Could not make reference to source blob: {relativePath}.");
                return;
            }

            try
            {
                await _Source.AcquireLeaseAsync(null); //Lease Id not used - cannot lease the source blob of a copy

                if (!_Source.Exists())
                {
                    _Logger.LogError($"Source blob does not exist: {relativePath}.");
                    return;
                }

                _Destination = _DestinationStorageAccountConfig.GetBlobIndirect(relativePath);

                if (_Destination == null)
                {
                    _Logger.LogError($"Could not make reference to destination blob: {relativePath}.");
                    return;
                }

                if (!_Destination.Exists())
                {
                    _Logger.LogWarning($"Destination blob did not exist: {relativePath}.");
                    _Destination.StartCopy(_Source);
                    return;
                }

                if (await _Source.ContentSame(_Destination))
                {
                    _Logger.LogInformation($"Destination blob already up to date.");
                    return;
                }

                _Logger.LogInformation($"Updating.");
                var destinationLease = await _Destination.AcquireLeaseAsync(null);
                _Destination.StartCopy(_Source, destAccessCondition: AccessCondition.GenerateLeaseCondition(destinationLease));
                _Logger.LogInformation($"Updated.");
            }
            finally
            {
                await _Source.ReleaseLease();
                await _Destination.ReleaseLease();
                _Logger.LogInformation($"Leases released.");
            }
        }
    }
}