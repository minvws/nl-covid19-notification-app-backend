// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public class Iso3166RegionCodeValidator
    {
        private static readonly HashSet<string> validValues;
        private static readonly string[] notSupportedDirectlyInDotNet = { "CY" };

        static Iso3166RegionCodeValidator()
        {
            validValues = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(x => new RegionInfo(x.LCID).TwoLetterISORegionName.ToUpper())
                .Concat(notSupportedDirectlyInDotNet)
                .Distinct()
                .ToHashSet();
        }

        public bool IsValid(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var parsed = code.Trim().ToUpper();

            if (code != parsed)
            {
                return false;
            }

            if (parsed.Length != 2)
            {
                return false;
            }

            return validValues.Contains(parsed);
        }
    }
}
