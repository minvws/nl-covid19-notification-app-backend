namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    /// <summary>
    /// Very granular if/until we design string-based config for DK processors.
    /// Add this interface to a section/group, then pass that AppSettingReader
    /// </summary>
    public interface IOutboundFixedCountriesOfInterestSetting
    {
        string[] CountriesOfInterest { get; }
    }
}