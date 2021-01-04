using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class CountryCodeListParser
    {
        private readonly Iso3166RegionCodeValidator _Validator = new Iso3166RegionCodeValidator();

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