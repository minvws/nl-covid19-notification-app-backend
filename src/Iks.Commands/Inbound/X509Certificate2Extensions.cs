// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public static class X509Certificate2Extensions
    {
        public static string ComputeSha256Hash(this X509Certificate2 cert)
        {   
            using var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(cert.RawData);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}