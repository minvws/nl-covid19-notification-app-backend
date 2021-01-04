// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    /// <summary>
    /// Too strict?
    /// </summary>
    public class Base64
    {
        public ValidationResult<byte[]> TryParseAndValidate(string value, int expectedLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new ValidationResult<byte[]>(new[] { "Value is null." });

            var buffer = new byte[expectedLength];
            if (!Convert.TryFromBase64String(value, new Span<byte>(buffer), out var bytesWritten))
                return new ValidationResult<byte[]>(new[] { $"Value is not valid base64 or result length exceeded {expectedLength}." });

            if (bytesWritten != expectedLength)
                return new ValidationResult<byte[]>(new[] { "Resulting byte array length not expected." });

            return new ValidationResult<byte[]>(buffer);
        }
    }
}