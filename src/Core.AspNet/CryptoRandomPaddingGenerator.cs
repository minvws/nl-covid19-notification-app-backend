// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public class CryptoRandomPaddingGenerator : IPaddingGenerator
    {
        readonly IRandomNumberGenerator _CryptoRng;

        public CryptoRandomPaddingGenerator(IRandomNumberGenerator cryptoRng)
        {
            _CryptoRng = cryptoRng ?? throw new ArgumentException(nameof(cryptoRng));
        }

        public string Generate(int length)
        {
            // Get randomness, convert to base64 then return the first length characters
            var randomBytes = _CryptoRng.NextByteArray(length);
            var base64 =  Convert.ToBase64String(randomBytes);
            return base64.Substring(0, length);
        }
    }
}