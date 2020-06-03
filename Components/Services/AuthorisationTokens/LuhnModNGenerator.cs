// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens
{
    public class LuhnModNGenerator
    {
        private readonly ILuhnModNConfig _Config;

        public LuhnModNGenerator(ILuhnModNConfig config)
        {
            _Config = config;
        }

        public string Next(Func<int,int> r)
        {
            var buffer = new char[_Config.ValueLength];
            for (var i = 0; i < _Config.ValueLength - 1; i++)
            {
                buffer[i] = _Config.CharacterSet[r(_Config.CharacterSet.Length - 1)];
            }
            buffer[_Config.ValueLength - 1] = GenerateCheckCharacter(buffer);
            return new string(buffer);
        }

        private char GenerateCheckCharacter(char[] input)
        {
            var factor = 2;
            var sum = 0;
            var n = _Config.CharacterSet.Length;

            for (var i = _Config.ValueLength - 2; i >= 0; i--)
            {
                var codePoint = Array.IndexOf(_Config.CharacterSet, input[i]);
                var addend = factor * codePoint;
                factor = (factor == 2) ? 1 : 2;
                addend = addend / n + addend % n;
                sum += addend;
            }

            var remainder = sum % n;
            var checkCodePoint = (n - remainder) % n;
            return _Config.CharacterSet[checkCodePoint];
        }
    }
}