// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        private readonly RNGCryptoServiceProvider csp;

        public RandomNumberGenerator()
        {
            csp = new RNGCryptoServiceProvider();
        }

        public string GenerateToken(int length = 6)
        {
            var allowedChars = "BCFGJLQRSTUVXYZ23456789".ToArray();
            var token = "";
            
            for (var x = 1; x<= length; x++)
            {
                var randomNumber = Next(0, allowedChars.Length - 1);
                token += allowedChars[randomNumber];
            }

            return token;
        }

        public int Next(int min, int max)
        {
            if (min >= max)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            // match Next of Random
            // where max is exclusive
            max -= 1;

            var bytes = new byte[sizeof(int)]; // 4 bytes
            csp.GetNonZeroBytes(bytes);
            var val = BitConverter.ToInt32(bytes);
            // constrain our values to between our min and max
            // https://stackoverflow.com/a/3057867/86411
            var result = ((val - min) % (max - min + 1) + (max - min + 1)) % (max - min + 1) + min;
            return result;
        }

        public byte[] GenerateKey(int keyLength = 32)
        {
            var randomBytes = new byte[keyLength];
            csp.GetBytes(randomBytes);
            return randomBytes;
        }
    }
}