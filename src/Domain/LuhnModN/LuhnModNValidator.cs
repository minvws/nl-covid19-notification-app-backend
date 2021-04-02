// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN
{
    public class LuhnModNValidator : ILuhnModNValidator
    {
        private readonly ILuhnModNConfig _config;

        public LuhnModNValidator(ILuhnModNConfig config)
        {
            _config = config;
        }

        public ILuhnModNConfig Config => _config;

        /// <summary>
        /// Validates value for LuhnModN 
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <returns>True if valid, otherwise false</returns>
        public bool Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var factor = 1;
            var sum = 0;
            for (var index = value.Length - 1; index >= 0; --index)
            {
                var codePoint = Array.IndexOf(_config.CharacterSet, value[index]);
                var addend = factor * codePoint;
                factor = factor == 2 ? 1 : 2;
                sum += addend / _config.CharacterSet.Length + addend % _config.CharacterSet.Length;
            }
            return sum % _config.CharacterSet.Length == 0;
        }
    }
}
