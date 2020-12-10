// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests
{
    public static class HmacSigner
    {
        //public static string Sign(in string keyString, in byte[] data)
        //{
        //    var key = Convert.FromBase64String(keyString);

        //    return Sign(key, data);
        //}

        public static string Sign(in byte[] key, in byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(data);
            var hashBase64 = Convert.ToBase64String(hash);

            return hashBase64;
        }
    }
}