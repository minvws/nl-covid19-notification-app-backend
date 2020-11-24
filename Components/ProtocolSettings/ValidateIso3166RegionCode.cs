using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings
{
    public class ValidateIso3166RegionCode
    {
        private static readonly HashSet<string> _ValidValues;
        private static readonly string[] _NotSupportedDirectlyInDotNet = { "CY" };

        static ValidateIso3166RegionCode()
        {
            _ValidValues = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(x => new RegionInfo(x.LCID).TwoLetterISORegionName.ToUpper())
                .Concat(_NotSupportedDirectlyInDotNet)
                .Distinct()
                .ToHashSet();
        }

        public bool IsValid(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var parsed = code.Trim().ToUpper();

            if (code != parsed)
                return false;

            if (parsed.Length != 2)
                return false;

            return _ValidValues.Contains(parsed);
        }
    }
}