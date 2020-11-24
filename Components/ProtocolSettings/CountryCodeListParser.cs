using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings
{
    public class CountryCodeListParser
    {
        private readonly ValidateIso3166RegionCode _Validator = new ValidateIso3166RegionCode();

        public string[] Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(nameof(value));

            var items = value.Split(",", StringSplitOptions.RemoveEmptyEntries);

            items = items.Select(x => x.Trim().ToUpper()).ToArray();

            if (items.Any(x => !_Validator.IsValid(x)))
                throw new ArgumentException("One or more country codes are not valid.", nameof(value));

            return items;
        }
    }
}