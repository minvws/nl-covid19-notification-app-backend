// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens
{
    public class LuhnModNValidator
    {
        private readonly ILuhnModNConfig _Config;

        public LuhnModNValidator(ILuhnModNConfig config)
        {
            _Config = config;
        }

        public bool Validate(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var factor = 1;
            var sum = 0;
            for (var index = value.Length - 1; index >= 0; --index)
            {
                var codePoint = Array.IndexOf(_Config.CharacterSet, value[index]);
                var addend = factor * codePoint;
                factor = factor == 2 ? 1 : 2;
                sum += addend / _Config.CharacterSet.Length + addend % _Config.CharacterSet.Length;
            }
            return sum % _Config.CharacterSet.Length == 0;
        }
    }
}