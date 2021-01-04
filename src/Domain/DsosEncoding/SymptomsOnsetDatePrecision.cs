namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding
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