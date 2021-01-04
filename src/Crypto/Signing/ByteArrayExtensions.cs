// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Handles zero-length.
        /// Strips leading zeros and returns a shorter array.
        /// </summary>
        public static byte[] StripLeadingZeros(this byte[] buffer)
        {
            if (buffer == null) 
                throw new ArgumentException(nameof(buffer)); //ncrunch: no coverage

            var i = 0;
            for (; i < buffer.Length && buffer[i] == 0; i++) ;

            if (i == 0)
                return buffer;

            if (i == buffer.Length)
                return new byte[1];

            var result = new byte[buffer.Length - i];
            Array.Copy(buffer, i, result, 0, buffer.Length - i);
            return result;
        }

        public static byte[] ToANS1Integer(this byte[] buffer)
        {
            if (buffer == null || buffer.Length > 32)
                throw new ArgumentException(nameof(buffer)); //ncrunch: no coverage

            var clean = buffer.StripLeadingZeros();

            // Length of the integer plus the 2 byte prefix (0x02 == integer, length)
            var l = clean.Length;

            // Extra 0x00 prefix if the first number is >= 0x80
            if (clean[0] >= 0x80)
                l++;

            var result = new byte[l+2];

            // Header
            result[0] = 0x02;
            result[1] = (byte) l;
            
            // Extra 0x0 if first byte >= 0x800
            if (clean[0] >= 0x80)
                result[2] = 0;

            // So now we have 0x02 (integer type), length and the clean string
            Array.Copy(clean, 0, result, clean[0] >= 0x80 ? 3 : 2, clean.Length);

            Debug.Assert(result[1] <= 0x21);

            return result;
        }
    }
}