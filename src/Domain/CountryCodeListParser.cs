// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class CountryCodeListParser
    {
        private readonly Iso3166RegionCodeValidator _validator = new Iso3166RegionCodeValidator();

        public string[] Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(nameof(value));

            var items = value.Split(",", StringSplitOptions.RemoveEmptyEntries);

            items = items.Select(x => x.Trim().ToUpper()).ToArray();

            if (items.Any(x => !_validator.IsValid(x)))
                throw new ArgumentException("One or more country codes are not valid.", nameof(value));

            return items;
        }
    }
}
