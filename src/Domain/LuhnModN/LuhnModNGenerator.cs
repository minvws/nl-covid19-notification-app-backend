// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
// using System.Diagnostics;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN
{
    public class LuhnModNGenerator : ILuhnModNGenerator
    {
        private readonly ILuhnModNConfig _config;

        public LuhnModNGenerator(ILuhnModNConfig config)
        {
            _config = config;
        }

        public string CalculateCheckCode(string key)
        {
            return $"{key}{GenerateCheckCharacter(key.ToCharArray())}";
        }

        public string Next(int length)
        {
            var r = new Random();
            var buffer = new char[_config.ValueLength];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = _config.CharacterSet[r.Next(_config.CharacterSet.Length)];
            }
            buffer[_config.ValueLength - 1] = GenerateCheckCharacter(buffer);
            return new string(buffer);
        }

        private char GenerateCheckCharacter(char[] input)
        {
            var factor = 2;
            var sum = 0;
            var n = _config.CharacterSet.Length;

            for (var i = _config.ValueLength - 2; i >= 0; i--)
            {
                var codePoint = Array.IndexOf(_config.CharacterSet, input[i]);
                var addend = factor * codePoint;
                factor = (factor == 2) ? 1 : 2;
                addend = addend / n + addend % n;
                sum += addend;
            }

            var remainder = sum % n;
            var checkCodePoint = (n - remainder) % n;

            return _config.CharacterSet[checkCodePoint];
        }
    }
}
