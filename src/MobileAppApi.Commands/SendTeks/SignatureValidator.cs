// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class SignatureValidator : ISignatureValidator
    {
        public bool Valid(byte[] signature, byte[] confirmationKey, byte[] data)
        {
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (confirmationKey == null) throw new ArgumentNullException(nameof(confirmationKey));
            if (data == null) throw new ArgumentNullException(nameof(data));

            using var hmac = new HMACSHA256(confirmationKey);
            var hash = hmac.ComputeHash(data);
            return hash.SequenceEqual(signature);
        }
    }
}