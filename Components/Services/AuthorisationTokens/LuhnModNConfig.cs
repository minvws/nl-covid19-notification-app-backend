// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens
{
    public class LuhnModNConfig : ILuhnModNConfig
    {
        /// <summary>
        /// Default for Workflow token
        /// </summary>
        public LuhnModNConfig() : this("ABCDEFGHJKLMNPRSTUVWXYZ23456789", 10) { }

        /// <summary>
        /// TODO load from config
        /// </summary>
        public LuhnModNConfig(string characterSet, int valueLength)
        {
            if (characterSet.Distinct().Count() != characterSet.Length)
                throw new ArgumentException("Duplicates characters.", nameof(characterSet));

            if (valueLength < 2)
                throw new ArgumentException();

            CharacterSet = characterSet.ToCharArray();
            ValueLength = valueLength;
        }

        public char[] CharacterSet { get; }
        public int ValueLength {get;}
    }
}