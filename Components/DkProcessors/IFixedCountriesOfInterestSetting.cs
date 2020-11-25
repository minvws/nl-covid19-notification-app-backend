namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    /// <summary>
    /// Very granular if/until we design string-based config for DK processors.
    /// Add this interface to a section/group, then pass that AppSettingReader
    /// </summary>
    public interface IFixedCountriesOfInterestSetting
    {
        string[] CountriesOfInterest { get; }
    }
}