using System.Linq;
using EFCore.BulkExtensions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class SubsetBulkArgs
    {
        public int BatchSize { get; set; } = 10000;
        public int TimeoutSeconds { get; set; } = 6000;
        public string[] PropertiesToInclude { get; set; } = new string[0];

        public BulkConfig ToBulkConfig()
            => new BulkConfig
            {
                BatchSize = BatchSize,
                BulkCopyTimeout = TimeoutSeconds,
                UseTempDB = true,
                PropertiesToInclude = PropertiesToInclude.ToList()
            };
    }
}