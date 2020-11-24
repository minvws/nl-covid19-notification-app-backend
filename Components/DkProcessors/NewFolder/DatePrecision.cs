using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    [Obsolete("Derivable from EncodedDsosType.")]
    public enum DatePrecision
    {
        Unknown,
        Range,
        
        /// <summary>
        /// Not encoded
        /// </summary>
        Exact
    }
}