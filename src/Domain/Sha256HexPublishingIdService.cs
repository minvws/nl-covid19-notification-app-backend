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
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }

            using var hasher = SHA256.Create();
            var id = hasher.ComputeHash(contents);

            var result = new StringBuilder(id.Length * 2);
            foreach (var i in id)
            {
                result.AppendFormat("{0:x2}", i);
            }

            return result.ToString();
        }
    }
}
