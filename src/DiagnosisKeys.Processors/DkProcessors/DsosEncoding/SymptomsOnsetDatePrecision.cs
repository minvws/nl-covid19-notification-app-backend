using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    public enum SymptomsOnsetDatePrecision
    {
        Unknown,
        Range,
        
        /// <summary>
        /// Not encoded => DSOS
        /// </summary>
        Exact
    }
}