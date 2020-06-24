// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    public class ResourceBundleValidator
    {
        private readonly HardCodedResourceBundleValidationConfig _Config = new HardCodedResourceBundleValidationConfig();

        public static bool IsBase64(string value)
        {
            //Convert.TryFromBase64String(value, new Span<byte>(), out int _);

            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                var _ = Convert.FromBase64String(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool CultureExists(string cultureName)
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
        }

        private bool LocalValid(string name)
            => !string.IsNullOrWhiteSpace(name) && _Config.Locales.Any(x => string.Equals(name, x, StringComparison.CurrentCultureIgnoreCase));

        public bool Valid(ResourceBundleArgs args)
        {
            if (args == null)
                return false;

            if (args.IsolationPeriodDays < 1) //TODO range?
                return false;

            if (args.Release.Year < 2020) //TODO range?
                return false;

            var locales = args.Text.Select(x => x.Key).ToArray();

            if (locales.Any(x => !LocalValid(x)))
                return false;

            if (locales.Distinct().Count() != locales.Length)
                return false;

            return args.Text.All(x => Valid(x.Value));
        }

        private bool Valid(Dictionary<string, string> args)
        {
            return args.All(val => IsBase64(val.Value));
        }
    }
}
