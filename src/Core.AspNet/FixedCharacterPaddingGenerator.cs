// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public class FixedCharacterPaddingGenerator : IPaddingGenerator
    {
        /// <summary>
        /// Character used for padding, must be a 1-byte character
        /// </summary>
        private const string PaddingCharacter = "=";

        public string Generate(int length)
        {
            return string.Concat(Enumerable.Repeat(PaddingCharacter, length));
        }
    }
}