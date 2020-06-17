using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets
{
    /// <summary>
    /// TODO incomplete.
    /// </summary>
    public class EksCreateJobOutputEntity
    {
        public int Id { get; set; }
        public DateTime Release { get; set; }
        public string Region { get; set; } = DefaultValues.Region;
        public byte[]? Content { get; set; }

        /// <summary>
        /// Metadata - Job Id.
        /// </summary>
        public string CreatingJobName { get; set; }

        /// <summary>
        /// Metadata - Job Id.
        /// </summary>
        public int CreatingJobQualifier { get; set; }
    }
}