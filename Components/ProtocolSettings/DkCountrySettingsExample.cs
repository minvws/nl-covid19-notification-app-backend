using System.Text;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings
{
    
    /// <summary>
    /// Do not use in production.
    /// Create specific classes for the required settings but ensure and invalid value is set for the default.
    /// </summary>
    public class DkCountrySettingsExample : AppSettingsReader
    {

        private readonly CountryCodeListParser _CountryCodeListParser = new CountryCodeListParser();

        public DkCountrySettingsExample(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string[] AcceptedCountries => _CountryCodeListParser.Parse(GetConfigValue(nameof(AcceptedCountries), "Not set so go boom!"));
    }
}
