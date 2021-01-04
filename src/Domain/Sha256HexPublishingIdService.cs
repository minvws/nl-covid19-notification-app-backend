// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class Sha256HexPublishingIdService : IPublishingIdService
    {
        public string Create(byte[] contents)
        {
            if (contents == null) throw new ArgumentNullException(nameof(contents));

            using var hasher = SHA256.Create();
            var id = hasher.ComputeHash(contents);

            var result = new StringBuilder(id.Length * 2);
            foreach (var i in id)
                result.AppendFormat("{0:x2}", i);

            return result.ToString();
        }

        //d65b6d0fb08646e8b947f460aa9d2998d22709c459bac9859189a7ae9727e494
        public bool Validate(string id)
        {
            const int sha256Length = 32;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            if(id.Length != sha256Length * 2)
                return false;

            for (var i = 0; i < id.Length; i += 2)
            {
                if (!int.TryParse(id.Substring(i, 2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out _))
                    return false;
            }

            return true;
        }
    }
}