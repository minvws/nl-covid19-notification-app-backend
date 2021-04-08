// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN
{
    public class LuhnModNConfig : ILuhnModNConfig
    {
        /// <summary>
        /// Default for PubTEK
        /// </summary>
        public LuhnModNConfig() : this("BCFGJLQRSTUVXYZ23456789", 7) { }

        /// <summary>
        /// TODO load from config
        /// </summary>
        public LuhnModNConfig(string characterSet, int valueLength)
        {
            if (characterSet.Distinct().Count() != characterSet.Length)
            {
                throw new ArgumentException("Duplicate characters.", nameof(characterSet));
            }

            if (valueLength < 2)
            {
                throw new ArgumentException($"{nameof(valueLength)} cannot be shorter than 2 positions");
            }

            CharacterSet = characterSet.ToCharArray();
            ValueLength = valueLength;
        }

        public char[] CharacterSet { get; }
        public int ValueLength { get; }
    }

}
