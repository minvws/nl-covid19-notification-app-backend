// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public class StandardRandomNumberGenerator : IRandomNumberGenerator
    {
        private readonly RNGCryptoServiceProvider _Random;

        public StandardRandomNumberGenerator()
        {
            _Random = new RNGCryptoServiceProvider();
        }

        /// <summary>
        /// Get integer between inclusive range
        /// Edge case of min == max to allow one value distributions to be 'randomised'
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int Next(int min, int max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException(nameof(min), "min cannot be greater than max");

            if (min == max)
                return min;

            var bytes = new byte[sizeof(int)]; // 4 bytes
            _Random.GetNonZeroBytes(bytes);
            var val = BitConverter.ToInt32(bytes);
            
            // constrain our values to between our min and max
            // https://stackoverflow.com/a/3057867/86411
            var result = ((val - min) % (max - min + 1) + max - min + 1) % (max - min + 1) + min;
            return result;
        }

        public byte[] NextByteArray(int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size));


            var randomBytes = new byte[size];
            _Random.GetBytes(randomBytes);
            return randomBytes;
        }
    }
}